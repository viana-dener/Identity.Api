using EBL.FIG.Common.Middleware.Lib.Notifications;
using EBL.FIG.Process.Identity.Application.Dto.Request.Auth;
using EBL.FIG.Process.Identity.Application.Services;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Interfaces;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EBL.FIG.Process.Identity.Tests.Application;

public class ForgotPasswordAppServiceTests
{
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<IUserDataRepository> _userRepoMock = new();
    private readonly Mock<ITenantDataRepository> _tenantRepoMock = new();
    private readonly Mock<IPasswordResetTokenDataRepository> _resetTokenRepoMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();
    private readonly Mock<IRequestTenantContext> _tenantContextMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();

    private ForgotPasswordAppService CreateSut()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
        _configurationMock.Setup(x => x["App:BaseUrl"]).Returns("https://app.example.com");

        return new ForgotPasswordAppService(
            _notifyMock.Object,
            _userRepoMock.Object,
            _tenantRepoMock.Object,
            _resetTokenRepoMock.Object,
            _emailSenderMock.Object,
            _localizationMock.Object,
            _tenantContextMock.Object,
            _configurationMock.Object,
            NullLogger<ForgotPasswordAppService>.Instance);
    }

    private static TenantEntity BuildTenant(int tenantId = 1)
    {
        var tenant = new TenantEntity("Tenant Test", "desc", "alias", null, null, null, 0);
        typeof(EBL.FIG.Process.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(tenant, tenantId);
        return tenant;
    }

    private static UserEntity BuildActiveUser(int tenantId = 1, int userId = 10)
    {
        var user = new UserEntity(tenantId, "Test User", "user@example.com", "hash", null, 0);
        typeof(EBL.FIG.Process.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(user, userId);
        return user;
    }

    [Fact]
    public async Task ForgotPassword_TenantNaoEncontrado_DeveRetornarMensagemGenerica()
    {
        _tenantRepoMock.Setup(x => x.GetByLoginIdentifierAsync(It.IsAny<string>(), default))
                       .ReturnsAsync((TenantEntity)null);

        var sut = CreateSut();
        var result = await sut.ForgotPasswordAsync(new ForgotPasswordRequest { LoginIdentifier = "nao@existe.com" }, default);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Message);
        _resetTokenRepoMock.Verify(x => x.CreateAsync(It.IsAny<PasswordResetTokenEntity>(), default), Times.Never);
        _emailSenderMock.Verify(x => x.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_UsuarioNaoEncontrado_DeveRetornarMensagemGenerica()
    {
        _tenantRepoMock.Setup(x => x.GetByLoginIdentifierAsync(It.IsAny<string>(), default))
                       .ReturnsAsync(BuildTenant());
        _userRepoMock.Setup(x => x.GetByNormalizedLoginAsync(1, It.IsAny<string>(), default))
                     .ReturnsAsync((UserEntity)null);

        var sut = CreateSut();
        var result = await sut.ForgotPasswordAsync(new ForgotPasswordRequest { LoginIdentifier = "nao@existe.com" }, default);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Message);
        _resetTokenRepoMock.Verify(x => x.CreateAsync(It.IsAny<PasswordResetTokenEntity>(), default), Times.Never);
        _emailSenderMock.Verify(x => x.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_RateLimitAtingido_DeveRetornarMensagemGenericaSemCriarToken()
    {
        var user = BuildActiveUser();
        _tenantRepoMock.Setup(x => x.GetByLoginIdentifierAsync(It.IsAny<string>(), default))
                       .ReturnsAsync(BuildTenant());
        _userRepoMock.Setup(x => x.GetByNormalizedLoginAsync(1, It.IsAny<string>(), default))
                     .ReturnsAsync(user);
        _resetTokenRepoMock.Setup(x => x.CountRecentByUserAsync(It.IsAny<int>(), 1, It.IsAny<DateTime>(), default))
                           .ReturnsAsync(5);

        var sut = CreateSut();
        var result = await sut.ForgotPasswordAsync(new ForgotPasswordRequest { LoginIdentifier = "user@example.com" }, default);

        Assert.NotNull(result);
        _resetTokenRepoMock.Verify(x => x.CreateAsync(It.IsAny<PasswordResetTokenEntity>(), default), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_LoginIdentifierValido_DeveCriarTokenEEnviarEmail()
    {
        var user = BuildActiveUser();
        _tenantRepoMock.Setup(x => x.GetByLoginIdentifierAsync(It.IsAny<string>(), default))
                       .ReturnsAsync(BuildTenant());
        _userRepoMock.Setup(x => x.GetByNormalizedLoginAsync(1, It.IsAny<string>(), default))
                     .ReturnsAsync(user);
        _resetTokenRepoMock.Setup(x => x.CountRecentByUserAsync(It.IsAny<int>(), 1, It.IsAny<DateTime>(), default))
                           .ReturnsAsync(0);
        _resetTokenRepoMock.Setup(x => x.CreateAsync(It.IsAny<PasswordResetTokenEntity>(), default))
                           .ReturnsAsync(true);
        _emailSenderMock.Setup(x => x.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
                        .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var result = await sut.ForgotPasswordAsync(new ForgotPasswordRequest { LoginIdentifier = "user@example.com" }, default);

        Assert.NotNull(result);
        _resetTokenRepoMock.Verify(x => x.CreateAsync(It.IsAny<PasswordResetTokenEntity>(), default), Times.Once);
        _emailSenderMock.Verify(x => x.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task ValidateResetToken_TokenInvalido_DeveRetornarIsValidFalse()
    {
        _resetTokenRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), default))
                           .ReturnsAsync((PasswordResetTokenEntity)null);

        var sut = CreateSut();
        var result = await sut.ValidateResetTokenAsync(new ValidateResetTokenRequest { Token = "token-invalido" }, default);

        Assert.False(result.IsValid);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact]
    public async Task ValidateResetToken_TokenValido_DeveRetornarIsValidTrue()
    {
        var user = BuildActiveUser();
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("token-valido"));
        var tokenEntity = new PasswordResetTokenEntity(1, 10, hash, DateTime.UtcNow.AddMinutes(10), 0);

        _resetTokenRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), default))
                           .ReturnsAsync(tokenEntity);
        _userRepoMock.Setup(x => x.GetByIdAsync(1, 10, default))
                     .ReturnsAsync(user);

        var sut = CreateSut();
        var result = await sut.ValidateResetTokenAsync(new ValidateResetTokenRequest { Token = "token-valido" }, default);

        Assert.True(result.IsValid);
    }
}
