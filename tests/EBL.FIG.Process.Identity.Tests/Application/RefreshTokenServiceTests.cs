using EBL.FIG.Common.Middleware.Lib.Notifications;
using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Services;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Interfaces;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace EBL.FIG.Process.Identity.Tests.Application;

public class RefreshTokenServiceTests
{
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<IRefreshTokenDataRepository> _refreshRepoMock = new();
    private readonly Mock<IRefreshTokenHasher> _hasherMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private RefreshTokenService CreateSut(JwtSettings settings = null)
    {
        settings ??= new JwtSettings { RefreshTokenExpirationDays = 7 };
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);

        return new RefreshTokenService(
            _notifyMock.Object,
            _refreshRepoMock.Object,
            _hasherMock.Object,
            _localizationMock.Object,
            NullLogger<RefreshTokenService>.Instance,
            Options.Create(settings));
    }

    [Fact]
    public async Task IssueAsync_DeveCriarTokenComHashEPersistir()
    {
        var hash = new byte[] { 1, 2, 3 };
        _hasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns(hash);
        _refreshRepoMock.Setup(x => x.CreateAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.IssueAsync(tenantId: 1, appId: 2, userId: 10, ct: default);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        _refreshRepoMock.Verify(x => x.CreateAsync(It.IsAny<RefreshTokenEntity>(), default), Times.Once);
    }

    [Fact]
    public async Task RotateAsync_ComTokenInvalido_DeveNotificarERetornarNull()
    {
        var hash = new byte[] { 9, 9, 9 };
        _hasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns(hash);
        _refreshRepoMock.Setup(x => x.GetByTokenHashAsync(hash, 1, default))
                        .ReturnsAsync((RefreshTokenEntity)null);

        var sut = CreateSut();
        var result = await sut.RotateAsync("token-invalido", tenantId: 1, ct: default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 401), Times.Once);
    }

    [Fact]
    public async Task RotateAsync_ComTokenRevogado_DeveNotificarERetornarNull()
    {
        var hash = new byte[] { 1, 2, 3 };
        _hasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns(hash);

        var revokedEntity = new RefreshTokenEntity(1, 2, 10, hash, DateTime.UtcNow.AddDays(7), 10);
        revokedEntity.Revoke(10);

        _refreshRepoMock.Setup(x => x.GetByTokenHashAsync(hash, 1, default)).ReturnsAsync(revokedEntity);

        var sut = CreateSut();
        var result = await sut.RotateAsync("token-revogado", tenantId: 1, ct: default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 401), Times.Once);
    }

    [Fact]
    public async Task RotateAsync_ComTokenValido_DeveRevogarAntigoECriarNovo()
    {
        var hash = new byte[] { 1, 2, 3 };
        var newHash = new byte[] { 4, 5, 6 };
        var callCount = 0;
        _hasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns(() => callCount++ == 0 ? hash : newHash);

        var activeEntity = new RefreshTokenEntity(1, 2, 10, hash, DateTime.UtcNow.AddDays(7), 10);
        _refreshRepoMock.Setup(x => x.GetByTokenHashAsync(hash, 1, default)).ReturnsAsync(activeEntity);
        _refreshRepoMock.Setup(x => x.RevokeAsync(activeEntity, default)).ReturnsAsync(true);
        _refreshRepoMock.Setup(x => x.CreateAsync(It.IsAny<RefreshTokenEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.RotateAsync("token-valido", tenantId: 1, ct: default);

        Assert.NotNull(result);
        Assert.NotEmpty(result.NewToken);
        _refreshRepoMock.Verify(x => x.RevokeAsync(activeEntity, default), Times.Once);
        _refreshRepoMock.Verify(x => x.CreateAsync(It.IsAny<RefreshTokenEntity>(), default), Times.Once);
    }

    [Fact]
    public async Task RevokeAllAsync_DeveInvocarRepositorioERretornarContagem()
    {
        _refreshRepoMock.Setup(x => x.RevokeAllByUserAsync(10, 1, 10, default)).ReturnsAsync(3);

        var sut = CreateSut();
        var count = await sut.RevokeAllAsync(userId: 10, tenantId: 1, revokedBy: 10, ct: default);

        Assert.Equal(3, count);
        _refreshRepoMock.Verify(x => x.RevokeAllByUserAsync(10, 1, 10, default), Times.Once);
    }
}
