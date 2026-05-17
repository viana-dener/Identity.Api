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
    private readonly Mock<IRefreshTokenDataRepository> _refreshTokenRepoMock = new();
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
            _refreshTokenRepoMock.Object,
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
    [Trait("Application", "ForgotPasswordAppService")]
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
    [Trait("Application", "ForgotPasswordAppService")]
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
    [Trait("Application", "ForgotPasswordAppService")]
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
    [Trait("Application", "ForgotPasswordAppService")]
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
    [Trait("Application", "ForgotPasswordAppService")]
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
    [Trait("Application", "ForgotPasswordAppService")]
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

    [Fact]
    [Trait("Application", "ForgotPasswordAppService")]
    public async Task ValidateResetToken_TokenExpirado_DeveRetornarIsValidFalseENotificar()
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("token-expirado"));
        var tokenEntity = new PasswordResetTokenEntity(1, 10, hash, DateTime.UtcNow.AddMinutes(10), 0);
        // Marca como usado para simular token inválido
        tokenEntity.MarkAsUsed(10);

        _resetTokenRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), default))
                           .ReturnsAsync(tokenEntity);

        var sut = CreateSut();
        var result = await sut.ValidateResetTokenAsync(new ValidateResetTokenRequest { Token = "token-expirado" }, default);

        Assert.False(result.IsValid);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact]
    [Trait("Application", "ForgotPasswordAppService")]
    public async Task ValidateResetToken_UsuarioInativo_DeveRetornarIsValidFalseENotificar()
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("token-ativo"));
        var tokenEntity = new PasswordResetTokenEntity(1, 10, hash, DateTime.UtcNow.AddMinutes(10), 0);

        var inactiveUser = new UserEntity(1, "Test User", "user@example.com", "hash", null, 0);
        typeof(EBL.FIG.Process.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(inactiveUser, 10);
        inactiveUser.Deactivate(0);

        _resetTokenRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), default))
                           .ReturnsAsync(tokenEntity);
        _userRepoMock.Setup(x => x.GetByIdAsync(1, 10, default))
                     .ReturnsAsync(inactiveUser);

        var sut = CreateSut();
        var result = await sut.ValidateResetTokenAsync(new ValidateResetTokenRequest { Token = "token-ativo" }, default);

        Assert.False(result.IsValid);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    #region ResetPasswordAsync

    [Fact]
    [Trait("Application", "ForgotPasswordAppService")]
    public async Task ResetPassword_TokenNaoEncontrado_DeveNotificar409ERetornarVazio()
    {
        _resetTokenRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), default))
                           .ReturnsAsync((PasswordResetTokenEntity)null);

        var sut = CreateSut();
        var result = await sut.ResetPasswordAsync(new ResetPasswordRequest { Token = "token-invalido", NewPassword = "Senha@123" }, "127.0.0.1", "agent", default);

        Assert.NotNull(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 409), Times.Once);
        _userRepoMock.Verify(x => x.UpdateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    [Fact]
    [Trait("Application", "ForgotPasswordAppService")]
    public async Task ResetPassword_TokenJaUtilizado_DeveNotificar410ERetornarVazio()
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("token-usado"));
        var tokenEntity = new PasswordResetTokenEntity(1, 10, hash, DateTime.UtcNow.AddMinutes(10), 0);
        tokenEntity.MarkAsUsed(10);

        _resetTokenRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), default))
                           .ReturnsAsync(tokenEntity);

        var sut = CreateSut();
        var result = await sut.ResetPasswordAsync(new ResetPasswordRequest { Token = "token-usado", NewPassword = "Senha@123" }, "127.0.0.1", "agent", default);

        Assert.NotNull(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _userRepoMock.Verify(x => x.UpdateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    [Fact]
    [Trait("Application", "ForgotPasswordAppService")]
    public async Task ResetPassword_UsuarioNaoEncontrado_DeveNotificar409ERetornarVazio()
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("token-valido"));
        var tokenEntity = new PasswordResetTokenEntity(1, 10, hash, DateTime.UtcNow.AddMinutes(10), 0);

        _resetTokenRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), default))
                           .ReturnsAsync(tokenEntity);
        _userRepoMock.Setup(x => x.GetByIdAsync(1, 10, default))
                     .ReturnsAsync((UserEntity)null);

        var sut = CreateSut();
        var result = await sut.ResetPasswordAsync(new ResetPasswordRequest { Token = "token-valido", NewPassword = "Senha@123" }, "127.0.0.1", "agent", default);

        Assert.NotNull(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 409), Times.Once);
        _userRepoMock.Verify(x => x.UpdateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    [Fact]
    [Trait("Application", "ForgotPasswordAppService")]
    public async Task ResetPassword_FalhaAoAtualizarSenha_DeveNotificar500ERetornarVazio()
    {
        var user = BuildActiveUser();
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("token-valido"));
        var tokenEntity = new PasswordResetTokenEntity(1, 10, hash, DateTime.UtcNow.AddMinutes(10), 0);

        _resetTokenRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), default))
                           .ReturnsAsync(tokenEntity);
        _userRepoMock.Setup(x => x.GetByIdAsync(1, 10, default))
                     .ReturnsAsync(user);
        _userRepoMock.Setup(x => x.UpdateAsync(user, default))
                     .ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.ResetPasswordAsync(new ResetPasswordRequest { Token = "token-valido", NewPassword = "Senha@123" }, "127.0.0.1", "agent", default);

        Assert.NotNull(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 500), Times.Once);
        _resetTokenRepoMock.Verify(x => x.UpdateAsync(It.IsAny<PasswordResetTokenEntity>(), default), Times.Never);
    }

    [Fact]
    [Trait("Application", "ForgotPasswordAppService")]
    public async Task ResetPassword_Sucesso_DeveAtualizarSenhaRevogarTokensEEnviarEmail()
    {
        var user = BuildActiveUser();
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("token-valido"));
        var tokenEntity = new PasswordResetTokenEntity(1, 10, hash, DateTime.UtcNow.AddMinutes(10), 0);

        _resetTokenRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), default))
                           .ReturnsAsync(tokenEntity);
        _userRepoMock.Setup(x => x.GetByIdAsync(1, 10, default))
                     .ReturnsAsync(user);
        _userRepoMock.Setup(x => x.UpdateAsync(user, default))
                     .ReturnsAsync(true);
        _resetTokenRepoMock.Setup(x => x.UpdateAsync(tokenEntity, default))
                           .ReturnsAsync(true);
        _refreshTokenRepoMock.Setup(x => x.RevokeAllByUserAsync(10, 1, 10, default))
                             .ReturnsAsync(1);
        _emailSenderMock.Setup(x => x.SendPasswordResetConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                        .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var result = await sut.ResetPasswordAsync(new ResetPasswordRequest { Token = "token-valido", NewPassword = "Senha@123" }, "127.0.0.1", "agent", default);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Message);
        _userRepoMock.Verify(x => x.UpdateAsync(user, default), Times.Once);
        _resetTokenRepoMock.Verify(x => x.UpdateAsync(tokenEntity, default), Times.Once);
        _refreshTokenRepoMock.Verify(x => x.RevokeAllByUserAsync(10, 1, 10, default), Times.Once);
        _emailSenderMock.Verify(x => x.SendPasswordResetConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    [Trait("Application", "ForgotPasswordAppService")]
    public async Task ResetPassword_FalhaNoEmail_DeveRetornarSucessoMesmoAsim()
    {
        var user = BuildActiveUser();
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("token-valido"));
        var tokenEntity = new PasswordResetTokenEntity(1, 10, hash, DateTime.UtcNow.AddMinutes(10), 0);

        _resetTokenRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), default))
                           .ReturnsAsync(tokenEntity);
        _userRepoMock.Setup(x => x.GetByIdAsync(1, 10, default))
                     .ReturnsAsync(user);
        _userRepoMock.Setup(x => x.UpdateAsync(user, default))
                     .ReturnsAsync(true);
        _resetTokenRepoMock.Setup(x => x.UpdateAsync(tokenEntity, default))
                           .ReturnsAsync(true);
        _refreshTokenRepoMock.Setup(x => x.RevokeAllByUserAsync(10, 1, 10, default))
                             .ReturnsAsync(1);
        _emailSenderMock.Setup(x => x.SendPasswordResetConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                        .ThrowsAsync(new Exception("SMTP error"));

        var sut = CreateSut();
        var result = await sut.ResetPasswordAsync(new ResetPasswordRequest { Token = "token-valido", NewPassword = "Senha@123" }, "127.0.0.1", "agent", default);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    [Trait("Application", "ForgotPasswordAppService")]
    public async Task ForgotPassword_FalhaAoPersistirToken_DeveRetornarMensagemGenerica()
    {
        var user = BuildActiveUser();
        _tenantRepoMock.Setup(x => x.GetByLoginIdentifierAsync(It.IsAny<string>(), default))
                       .ReturnsAsync(BuildTenant());
        _userRepoMock.Setup(x => x.GetByNormalizedLoginAsync(1, It.IsAny<string>(), default))
                     .ReturnsAsync(user);
        _resetTokenRepoMock.Setup(x => x.CountRecentByUserAsync(It.IsAny<int>(), 1, It.IsAny<DateTime>(), default))
                           .ReturnsAsync(0);
        _resetTokenRepoMock.Setup(x => x.CreateAsync(It.IsAny<PasswordResetTokenEntity>(), default))
                           .ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.ForgotPasswordAsync(new ForgotPasswordRequest { LoginIdentifier = "user@example.com" }, default);

        Assert.NotNull(result);
        _emailSenderMock.Verify(x => x.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    [Trait("Application", "ForgotPasswordAppService")]
    public async Task ForgotPassword_FalhaNoEmail_DeveRetornarMensagemGenerica()
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
                        .ThrowsAsync(new Exception("SMTP error"));

        var sut = CreateSut();
        var result = await sut.ForgotPasswordAsync(new ForgotPasswordRequest { LoginIdentifier = "user@example.com" }, default);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Message);
    }

    #endregion
}
