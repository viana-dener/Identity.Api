using EBL.FIG.Common.Middleware.Lib.Notifications;
using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Interfaces;
using EBL.FIG.Process.Identity.Application.Services;
using EBL.FIG.Process.Identity.Domain.Base;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Interfaces;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using EBL.FIG.Process.Identity.Domain.Tools.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace EBL.FIG.Process.Identity.Tests.Application;

public class JwtTokenServiceTests
{
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<IUserRoleDataRepository> _userRoleRepoMock = new();
    private readonly Mock<IRolePermissionDataRepository> _rolePermissionRepoMock = new();
    private readonly Mock<ITenantDataRepository> _tenantRepoMock = new();
    private readonly Mock<IJwtKeyDataRepository> _jwtKeyRepoMock = new();
    private readonly Mock<ISecretProvider> _secretProviderMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private const int TenantId = 1;
    private const int UserId = 10;
    private const string MasterKey = "masterkey-32-chars-for-aes256-ok!";

    private static readonly (string PublicKeyPem, string PrivateKeyPem) _keyPair = CryptoRSA.GenerateKeyPair(2048);
    private static readonly string _encryptedPrivateKey = CryptoRSA.EncryptPrivateKey(_keyPair.PrivateKeyPem, MasterKey);

    public JwtTokenServiceTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns<string, object[]>((k, _) => k);
    }

    private JwtTokenService CreateSut(JwtSettings settings = null)
    {
        settings ??= new JwtSettings
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationMinutes = 15
        };

        return new JwtTokenService(
            _notifyMock.Object,
            _userRoleRepoMock.Object,
            _rolePermissionRepoMock.Object,
            _tenantRepoMock.Object,
            _jwtKeyRepoMock.Object,
            _secretProviderMock.Object,
            _localizationMock.Object,
            NullLogger<JwtTokenService>.Instance,
            Options.Create(settings));
    }

    private static UserEntity BuildUser(int id = UserId, int tenantId = TenantId)
    {
        var user = new UserEntity(tenantId, "Test User", "test@email.com", "hash", null, 1);
        typeof(Entity).GetProperty("Id")!.SetValue(user, id);
        return user;
    }

    private JwtKeyEntity BuildJwtKey(int id = 1, string privateKeyEncrypted = null)
    {
        var key = new JwtKeyEntity(TenantId, _keyPair.PublicKeyPem, privateKeyEncrypted ?? _encryptedPrivateKey, UserId);
        typeof(Entity).GetProperty("Id")!.SetValue(key, id);
        return key;
    }

    private static RoleEntity BuildRole(int id = 1, string name = "admin")
    {
        var role = new RoleEntity(TenantId, 2, name, "Descrição", UserId);
        typeof(Entity).GetProperty("Id")!.SetValue(role, id);
        return role;
    }

    private static ResourceEntity BuildResource(int id = 1, string name = "users")
    {
        var resource = new ResourceEntity(TenantId, 2, name, "Descrição", UserId);
        typeof(Entity).GetProperty("Id")!.SetValue(resource, id);
        return resource;
    }

    private static ActionEntity BuildAction(int id = 1, string name = "read")
    {
        var action = new ActionEntity(TenantId, 2, name, "Descrição", UserId);
        typeof(Entity).GetProperty("Id")!.SetValue(action, id);
        return action;
    }

    #region GenerateAccessTokenAsync - Sucesso

    [Fact(DisplayName = "GenerateAccessTokenAsync - Deve gerar token com sucesso sem roles e permissões")]
    [Trait("Application", "JwtTokenService")]
    public async Task GenerateAccessTokenAsync_SemRoles_DeveGerarToken()
    {
        var user = BuildUser();
        var jwtKey = BuildJwtKey();

        _userRoleRepoMock.Setup(x => x.GetByUserIdAsync(UserId, default)).ReturnsAsync([]);
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync(jwtKey);
        _secretProviderMock.Setup(x => x.GetMasterKey()).Returns(MasterKey);

        var sut = CreateSut();
        var (token, expiresAt) = await sut.GenerateAccessTokenAsync(user, default);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(expiresAt > DateTime.UtcNow);
    }

    [Fact(DisplayName = "GenerateAccessTokenAsync - Deve gerar token com roles e permissões no payload")]
    [Trait("Application", "JwtTokenService")]
    public async Task GenerateAccessTokenAsync_ComRolesEPermissoes_DeveGerarToken()
    {
        var user = BuildUser();
        var jwtKey = BuildJwtKey();
        var role = BuildRole();
        var userRole = new UserRoleEntity(TenantId, 2, UserId, role.Id);
        typeof(UserRoleEntity).GetProperty("Role")!.SetValue(userRole, role);

        var resource = BuildResource();
        var action = BuildAction();
        var rolePermission = new RolePermissionEntity(TenantId, 2, role.Id, resource.Id, action.Id);
        typeof(RolePermissionEntity).GetProperty("Resource")!.SetValue(rolePermission, resource);
        typeof(RolePermissionEntity).GetProperty("Action")!.SetValue(rolePermission, action);

        _userRoleRepoMock.Setup(x => x.GetByUserIdAsync(UserId, default)).ReturnsAsync([userRole]);
        _rolePermissionRepoMock.Setup(x => x.GetByRoleAsync(role.Id, TenantId, default)).ReturnsAsync([rolePermission]);
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync(jwtKey);
        _secretProviderMock.Setup(x => x.GetMasterKey()).Returns(MasterKey);

        var sut = CreateSut();
        var (token, expiresAt) = await sut.GenerateAccessTokenAsync(user, default);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(expiresAt > DateTime.UtcNow);
    }

    [Fact(DisplayName = "GenerateAccessTokenAsync - Deve respeitar o tempo de expiração configurado")]
    [Trait("Application", "JwtTokenService")]
    public async Task GenerateAccessTokenAsync_DeveRespeitarExpiracao()
    {
        var user = BuildUser();
        var jwtKey = BuildJwtKey();
        var settings = new JwtSettings { Issuer = "issuer", Audience = "audience", AccessTokenExpirationMinutes = 30 };

        _userRoleRepoMock.Setup(x => x.GetByUserIdAsync(UserId, default)).ReturnsAsync([]);
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync(jwtKey);
        _secretProviderMock.Setup(x => x.GetMasterKey()).Returns(MasterKey);

        var before = DateTime.UtcNow;
        var sut = CreateSut(settings);
        var (token, expiresAt) = await sut.GenerateAccessTokenAsync(user, default);

        Assert.NotNull(token);
        Assert.True(expiresAt >= before.AddMinutes(29));
        Assert.True(expiresAt <= before.AddMinutes(31));
    }

    #endregion

    #region GenerateAccessTokenAsync - Insucesso

    [Fact(DisplayName = "GenerateAccessTokenAsync - Deve retornar null e notificar quando não há chave JWT ativa")]
    [Trait("Application", "JwtTokenService")]
    public async Task GenerateAccessTokenAsync_SemChaveAtiva_DeveRetornarNullENotificar()
    {
        var user = BuildUser();

        _userRoleRepoMock.Setup(x => x.GetByUserIdAsync(UserId, default)).ReturnsAsync([]);
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync((JwtKeyEntity)null);

        var sut = CreateSut();
        var (token, expiresAt) = await sut.GenerateAccessTokenAsync(user, default);

        Assert.Null(token);
        Assert.Equal(DateTime.MinValue, expiresAt);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 500), Times.Once);
    }

    [Fact(DisplayName = "GenerateAccessTokenAsync - Deve retornar null e notificar quando master key está ausente")]
    [Trait("Application", "JwtTokenService")]
    public async Task GenerateAccessTokenAsync_MasterKeyAusente_DeveRetornarNullENotificar()
    {
        var user = BuildUser();
        var jwtKey = BuildJwtKey();

        _userRoleRepoMock.Setup(x => x.GetByUserIdAsync(UserId, default)).ReturnsAsync([]);
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync(jwtKey);
        _secretProviderMock.Setup(x => x.GetMasterKey()).Returns(string.Empty);

        var sut = CreateSut();
        var (token, expiresAt) = await sut.GenerateAccessTokenAsync(user, default);

        Assert.Null(token);
        Assert.Equal(DateTime.MinValue, expiresAt);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 500), Times.Once);
    }

    [Fact(DisplayName = "GenerateAccessTokenAsync - Deve retornar null e notificar quando chave privada é inválida")]
    [Trait("Application", "JwtTokenService")]
    public async Task GenerateAccessTokenAsync_ChavePrivadaInvalida_DeveRetornarNullENotificar()
    {
        var user = BuildUser();
        var jwtKey = BuildJwtKey(privateKeyEncrypted: Convert.ToBase64String(new byte[48]));

        _userRoleRepoMock.Setup(x => x.GetByUserIdAsync(UserId, default)).ReturnsAsync([]);
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync(jwtKey);
        _secretProviderMock.Setup(x => x.GetMasterKey()).Returns(MasterKey);

        var sut = CreateSut();
        var (token, expiresAt) = await sut.GenerateAccessTokenAsync(user, default);

        Assert.Null(token);
        Assert.Equal(DateTime.MinValue, expiresAt);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 500), Times.Once);
    }

    [Fact(DisplayName = "GenerateAccessTokenAsync - Deve retornar null e notificar quando master key é null")]
    [Trait("Application", "JwtTokenService")]
    public async Task GenerateAccessTokenAsync_MasterKeyNull_DeveRetornarNullENotificar()
    {
        var user = BuildUser();
        var jwtKey = BuildJwtKey();

        _userRoleRepoMock.Setup(x => x.GetByUserIdAsync(UserId, default)).ReturnsAsync([]);
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync(jwtKey);
        _secretProviderMock.Setup(x => x.GetMasterKey()).Returns((string)null);

        var sut = CreateSut();
        var (token, expiresAt) = await sut.GenerateAccessTokenAsync(user, default);

        Assert.Null(token);
        Assert.Equal(DateTime.MinValue, expiresAt);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 500), Times.Once);
    }

    [Fact(DisplayName = "GenerateAccessTokenAsync - Deve continuar geração quando falha ao buscar roles (graceful degradation)")]
    [Trait("Application", "JwtTokenService")]
    public async Task GenerateAccessTokenAsync_FalhaAoBuscarRoles_DeveGerarTokenSemRoles()
    {
        var user = BuildUser();
        var jwtKey = BuildJwtKey();

        _userRoleRepoMock.Setup(x => x.GetByUserIdAsync(UserId, default)).ThrowsAsync(new Exception("Erro de banco"));
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync(jwtKey);
        _secretProviderMock.Setup(x => x.GetMasterKey()).Returns(MasterKey);

        var sut = CreateSut();
        var (token, expiresAt) = await sut.GenerateAccessTokenAsync(user, default);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    #endregion
}
