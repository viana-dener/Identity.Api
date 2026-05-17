using AutoMapper;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Dto.Request.Auth;
using EBL.FIG.Process.Identity.Application.Dto.Response.Auth;
using EBL.FIG.Process.Identity.Application.Interfaces;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Helpers;
using EBL.FIG.Process.Identity.Domain.Interfaces;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using EBL.FIG.Process.Identity.Domain.Tools.Cryptography;
using EBL.FIG.Process.Identity.Infra.Data.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace EBL.FIG.Process.Identity.Application.Services;

public class AuthAppService : IAuthAppService
{
    private readonly INotify _notify;
    private readonly IMapper _mapper;
    private readonly IRolePermissionDataRepository _rolePermissionRepo;
    private readonly IRefreshTokenDataRepository _refreshRepo;
    private readonly IUserDataRepository _userRepo;
    private readonly IUserRoleDataRepository _userRoleRepo;
    private readonly ITenantDataRepository _tenantRepo;
    private readonly ILocalizationService _localization;
    private readonly ICurrentUserService _currentUser;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthAppService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IJwtKeyDataRepository _jwtKeyRepo;
    private readonly IdentityDbContext _dbContext;
    private readonly ISecretProvider _secretProvider;
    private readonly IRequestTenantContext _requestTenantContext;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private int TenantId { get; set; }

    public AuthAppService(
        INotify notify,
        IMapper mapper,
        IRolePermissionDataRepository rolePermissionRepo,
        IRefreshTokenDataRepository refreshRepo,
        IUserDataRepository userRepo,
        IUserRoleDataRepository userRoleRepo,
        ITenantDataRepository tenantRepo,
        ILocalizationService localization,
        ICurrentUserService currentUser,
        IOptions<JwtSettings> jwtOptions,
        ILogger<AuthAppService> logger,
        IConfiguration configuration,
        IJwtKeyDataRepository jwtKeyRepo,
        IdentityDbContext dbContext,
        ISecretProvider secretProvider,
        IRequestTenantContext requestTenantContext,
        IRefreshTokenHasher refreshTokenHasher)
    {
        _notify = notify;
        _mapper = mapper;
        _rolePermissionRepo = rolePermissionRepo;
        _refreshRepo = refreshRepo;
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _tenantRepo = tenantRepo;
        _localization = localization;
        _currentUser = currentUser;
        _jwtSettings = jwtOptions.Value;
        _logger = logger;
        _configuration = configuration;
        _jwtKeyRepo = jwtKeyRepo;
        _dbContext = dbContext;
        _secretProvider = secretProvider;
        _requestTenantContext = requestTenantContext;
        _refreshTokenHasher = refreshTokenHasher;
        TenantId = _currentUser.GetTenantId();
    }

    public async Task<AuthDetailResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        // Validar tenantId
        if (request.TenantId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Register.InvalidTenantId"), 400);
            return null;
        }

        _requestTenantContext.SetTenantId(request.TenantId);

        var exists = await _userRepo.ExistsByNameAsync(TenantId, request.Name, ct);
        if (exists)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Register.NameAlreadyExists"), 409);
            return null;
        }

        // Hash da senha
        var passwordHash = DomainExtensions.HashClientSecret(request.Secret);

        // Criar entidade de usuário
        var user = new UserEntity(request.TenantId, request.Name, request.Name, passwordHash, request.UrlImage, 0);

        // Persistir via repositório
        var created = await _userRepo.CreateAsync(user, ct);
        if (!created)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Register.FailedToCreateUser"), 500);
            return null;
        }

        // Retornar sem tokens (login separado)
        return new AuthDetailResponse
        {
            RoleId = user.UserRoles.FirstOrDefault()?.RoleId ?? 0,
            RoleName = user.UserRoles.FirstOrDefault()?.Role?.Name,
            TenantId = user.TenantId,
            UserId = user.Id,
            UserName = user.Name
        };
    }

    public async Task<AuthDetailResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var tenant = await _tenantRepo.GetByLoginIdentifierAsync(request.LoginIdentifier, ct);
        if (tenant == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.UserNotAssociateInTenant"), 401);
            return null;
        }

        _requestTenantContext.SetTenantId(tenant.Id);

        var user = await _userRepo.GetByNormalizedLoginAsync(tenant.Id, request.LoginIdentifier, ct);
        if (user == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Login.InvalidCredentials"), 401);
            return null;
        }

        if (!DomainExtensions.VerifyClientSecret(user.PasswordHash, request.Password))
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Login.InvalidCredentials"), 401);
            return null;
        }

        // Obter AppId das roles do usuário
        var userRole = user.UserRoles?.FirstOrDefault();
        if (userRole == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Login.UserWithoutRole"), 403);
            return null;
        }

        // Gera tokens (usa chave RSA do tenant)
        var accessToken = await GenerateAccessTokenAsync(user, ct);
        if (accessToken.Token == null)
        {
            // Erro já notificado
            return null;
        }

        // Atualizar LastAccessAt do usuário (não bloquear login em caso de falha)
        try
        {
            user.UpdateLastAccess();
            await _userRepo.UpdateAsync(user, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao atualizar LastAccessAt para o usuário {UserId}", user.Id);
        }

        var refreshTokenValue = GenerateRefreshTokenValue();
        var refreshTokenHash = _refreshTokenHasher.Hash(refreshTokenValue);

        var refreshTokenEntity = new RefreshTokenEntity(user.TenantId, userRole.AppId, user.Id, refreshTokenHash, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays), user.Id);
        await _refreshRepo.CreateAsync(refreshTokenEntity, ct);

        return new AuthDetailResponse
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
            TenantId = user.TenantId,
            TenantName = user.Tenant.Name,
            AppId = userRole.AppId,
            AppName = userRole.App?.Name,
            UserId = user.Id,
            UserName = user.Name,
            RoleId = user.UserRoles.FirstOrDefault()?.RoleId ?? 0,
            RoleName = user.UserRoles.FirstOrDefault()?.Role?.Name
        };
    }

    public async Task<AuthDetailResponse> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        if (request.TenantId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Refresh.InvalidTenantId"), 400);
            return null;
        }

        _requestTenantContext.SetTenantId(request.TenantId);

        var refreshTokenHash = _refreshTokenHasher.Hash(request.RefreshToken);
        var refreshTokenEntity = await _refreshRepo.GetByTokenHashAsync(refreshTokenHash, request.TenantId, ct);
        if (refreshTokenEntity == null || !refreshTokenEntity.IsActive())
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Refresh.InvalidRefreshToken"), 401);
            return null;
        }

        // Encontrar usuário
        var user = await _userRepo.GetByIdAsync(request.TenantId, refreshTokenEntity.UserId, ct);
        if (user == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Refresh.UserNotFound"), 410);
            return null;
        }

        // Rotação: revogar token antigo e criar novo
        // Usar o mesmo AppId do refresh token atual
        int appId = refreshTokenEntity.AppId;

        refreshTokenEntity.Revoke(user.Id);
        await _refreshRepo.RevokeAsync(refreshTokenEntity, ct);

        var newRefreshValue = GenerateRefreshTokenValue();
        var newRefreshHash = _refreshTokenHasher.Hash(newRefreshValue);
        var newRefresh = new RefreshTokenEntity(request.TenantId, appId, user.Id, newRefreshHash, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays), user.Id);
        await _refreshRepo.CreateAsync(newRefresh, ct);

        var accessToken = await GenerateAccessTokenAsync(user, ct);

        return new AuthDetailResponse
        {
            AccessToken = accessToken.Token,
            RefreshToken = newRefreshValue,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshTokenExpiresAt = newRefresh.ExpiresAt,
            RoleId = user.UserRoles.FirstOrDefault()?.RoleId ?? 0,
            RoleName = user.UserRoles.FirstOrDefault()?.Role?.Name,
            TenantId = user.TenantId,
            TenantName = user.Tenant.Name,
            UserId = user.Id,
            UserName = user.Name
        };
    }

    private async Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(UserEntity user, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var tenant = await _tenantRepo.GetByIdAsync(user.TenantId, ct);
        // Gera claims básicos
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("tenantId", user.TenantId.ToString()),
            new("appId", user.UserRoles.FirstOrDefault()?.AppId.ToString() ?? "0"),
            new(JwtRegisteredClaimNames.Name, user.Name ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        string permissionsJson = null;

        // Adicionar roles e permissões ao token para autorização stateless
        try
        {
            // Buscar roles do usuário
            var userRoles = await _userRoleRepo.GetByUserIdAsync(user.Id, ct);

            var roleNames = userRoles?
                .Where(r => r?.Role != null)
                .Select(r => r.Role.Name?.Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.ToLower())
                .Distinct()
                .ToList() ?? [];

            foreach (var roleName in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
                claims.Add(new Claim("role", roleName));
            }

            // Coletar permissões de cada role agrupadas por recurso
            var permissionsByResource = new Dictionary<string, HashSet<string>>();

            foreach (var userRole in userRoles ?? Enumerable.Empty<UserRoleEntity>())
            {
                if (userRole?.Role == null) continue;
                var roleId = userRole.Role.Id;
                var rolePerms = await _rolePermissionRepo.GetByRoleAsync(roleId, user.TenantId, ct);
                if (rolePerms == null || !rolePerms.Any()) continue;

                foreach (var rp in rolePerms)
                {
                    var resource = rp.Resource?.Name?.Trim();
                    var action = rp.Action?.Name?.Trim();
                    if (string.IsNullOrWhiteSpace(resource) || string.IsNullOrWhiteSpace(action)) continue;

                    var resourceKey = resource.ToLower();
                    var actionKey = action.ToLower();

                    if (!permissionsByResource.TryGetValue(resourceKey, out HashSet<string> actions))
                    {
                        actions = new HashSet<string>();
                        permissionsByResource[resourceKey] = actions;
                    }

                    actions.Add(actionKey);
                }
            }

            // Serializar o dicionário para JSON e guardar em variável (não adicionar como claim string)
            if (permissionsByResource.Any())
            {
                // Converter HashSet<string> para List<string> para serialização previsível
                var serializable = permissionsByResource.ToDictionary(k => k.Key, x => x.Value.OrderBy(x => x).ToList());
                permissionsJson = JsonSerializer.Serialize(serializable);
                _logger.LogDebug("Permissions incluídas no token para user {UserId}: {ResourceCount} recursos", user.Id, serializable.Count);
                // Não adicionar como Claim string para evitar escape no payload
            }
            else
            {
                _logger.LogWarning("Nenhuma permissão encontrada para user {UserId}", user.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao incluir roles/permissions no token para user {UserId}", user.Id);
            // Não falhar a geração do token por causa das claims de permissão; conceder token básico
        }

        // Carregar chave ativa do tenant
        var keyEntity = await _jwtKeyRepo.GetActiveKeyAsync(user.TenantId, ct);
        if (keyEntity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Token.NoActiveKey"), 500);
            _logger.LogError("Nenhuma chave JWT ativa encontrada para tenant {TenantId}", user.TenantId);
            return (null, DateTime.MinValue);
        }

        // Obter chave mestra do provider de segredos
        var masterKey = _secretProvider.GetMasterKey();
        if (string.IsNullOrWhiteSpace(masterKey))
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Token.EncryptionKeyMissing"), 500);
            _logger.LogError("Master key for JWT decryption not available");
            return (null, DateTime.MinValue);
        }

        string privatePem;
        try
        {
            privatePem = CryptoRSA.DecryptPrivateKey(keyEntity.PrivateKeyEncrypted, masterKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao descriptografar chave privada para tenant {TenantId}", user.TenantId);
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Token.InvalidPrivateKey"), 500);
            return (null, DateTime.MinValue);
        }

        // Import private key into RSA
        try
        {
            // Não usar 'using' aqui: precisamos garantir que a instância RSA permaneça viva
            // durante a operação de assinatura (WriteToken). Alguns provedores internos
            // podem acessar o objeto RSA durante a escrita do token e provocar
            // ObjectDisposedException se ele for descartado antes.
            var rsa = RSA.Create();
            try
            {
                var privateKeyBytes = CryptoRSA.ExtractBytesFromPem(privatePem, "PRIVATE KEY");
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

                var rsaKey = new RsaSecurityKey(rsa) { KeyId = keyEntity.KeyId.ToString() };
                // Evitar cache de SignatureProviders que podem reter referência ao objeto RSA e
                // causar ObjectDisposedException em chamadas subsequentes. Garantir que cada
                // geração de token use um provider fresco.
                rsaKey.CryptoProviderFactory = new CryptoProviderFactory
                {
                    CacheSignatureProviders = false
                };
                var creds = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    notBefore: now,
                    expires: expires,
                    signingCredentials: creds
                );

                // Se temos permissionsJson, inserir como objeto JSON no payload para evitar escaping
                if (!string.IsNullOrWhiteSpace(permissionsJson))
                {
                    try
                    {
                        // Deserializar para Dictionary<string, List<string>> e adicionar diretamente ao payload
                        var permissionsDict = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(permissionsJson);
                        token.Payload["permissions"] = permissionsDict;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Falha ao parsear permissionsJson antes de inserir no payload");
                    }
                }

                var handler = new JwtSecurityTokenHandler();
                var tokenString = handler.WriteToken(token);
                return (tokenString, expires);
            }
            finally
            {
                // Garantir liberação explícita após a escrita do token
                try { rsa.Dispose(); } catch { }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar token JWT para tenant {TenantId}", user.TenantId);
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Token.CreateFailed"), 500);
            return (null, DateTime.MinValue);
        }
    }

    private string GenerateRefreshTokenValue()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=');
    }
}
