using EBL.FIG.Common.Middleware.Lib.Notifications;
using EBL.FIG.Process.Identity.Application.Interfaces;
using EBL.FIG.Process.Identity.Application.Services;
using EBL.FIG.Process.Identity.Domain.Base;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Interfaces;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using EBL.FIG.Process.Identity.Domain.Services;
using EBL.FIG.Process.Identity.Domain.Validators.Action;
using EBL.FIG.Process.Identity.Domain.Validators.App;
using EBL.FIG.Process.Identity.Domain.Validators.Job;
using EBL.FIG.Process.Identity.Domain.Validators.Jwt;
using EBL.FIG.Process.Identity.Domain.Validators.Resource;
using EBL.FIG.Process.Identity.Domain.Validators.Role;
using EBL.FIG.Process.Identity.Domain.Validators.RolePermission;
using EBL.FIG.Process.Identity.Domain.Validators.Tenant;
using EBL.FIG.Process.Identity.Domain.Validators.User;
using EBL.FIG.Process.Identity.Domain.Validators.UserRole;
using EBL.FIG.Process.Identity.Infra.Data.Context;
using EBL.FIG.Process.Identity.Infra.Data.Interceptors;
using EBL.FIG.Process.Identity.Infra.Data.Providers;
using EBL.FIG.Process.Identity.Infra.Data.Repository;
using EBL.FIG.Process.Identity.Infra.Job.HostedServices;
using EBL.FIG.Process.Identity.Infra.Job.Interfaces;
using EBL.FIG.Process.Identity.Infra.Job.Jobs.Maintenance;
using EBL.FIG.Process.Identity.Infra.Job.Jobs.Security;
using EBL.FIG.Process.Identity.Infra.Job.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EBL.FIG.Process.Identity.Infra.IoC;

/// <summary>
/// Configuração centralizada de injeção de dependências para todas as camadas do Identity.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra todos os serviços da aplicação no container de DI.
    /// </summary>
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        // Notificações (Scoped para manter estado durante a requisição)
        services.AddScoped<INotify, Notify>();

        // Context de Tenant para Request (Scoped)
        services.AddScoped<IRequestTenantContext, RequestTenantContext>();

        // Interceptors do Entity Framework (Scoped)
        services.AddScoped<TenantSessionConnectionInterceptor>();
        services.AddScoped<TenantSessionCommandInterceptor>();
        services.AddScoped<TelemetryInterceptor>();

        // Serviços de Infraestrutura Base
        services.AddScoped<ISecretProvider, EnvironmentSecretProvider>();
        services.AddScoped<IFileValidationService, FileValidationService>();

        // Validators (Scoped)
        services.AddScoped<IValidator<UserRoleEntity>, UserRoleValidator>();
        services.AddScoped<IValidator<RolePermissionEntity>, RolePermissionValidator>();

        services.AddScoped<IEntityDomainValidator<ActionEntity>, ActionValidator>();
        services.AddScoped<IEntityDomainValidator<AppEntity>, AppValidator>();
        services.AddScoped<IEntityDomainValidator<ResourceEntity>, ResourceValidator>();
        services.AddScoped<IEntityDomainValidator<RoleEntity>, RoleValidator>();
        services.AddScoped<IEntityDomainValidator<TenantEntity>, TenantValidator>();
        services.AddScoped<IEntityDomainValidator<UserEntity>, UserValidator>();
        services.AddScoped<IEntityDomainValidator<JobDefinitionEntity>, JobDefinitionValidator>();
        services.AddScoped<IEntityDomainValidator<JwtKeyEntity>, JwtKeyValidator>();
        services.AddScoped<IEntityDomainValidator<UserRoleEntity>, UserRoleValidator>();

        // Application - Common Services
        // Application - App Services
        services.AddScoped<IActionAppService, ActionAppService>();
        services.AddScoped<IAppAppService, AppAppService>();
        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IJwtKeyAppService, JwtKeyAppService>();
        services.AddScoped<IJobAppService, JobAppService>();
        services.AddScoped<IResourceAppService, ResourceAppService>();
        services.AddScoped<IRoleAppService, RoleAppService>();
        services.AddScoped<IRolePermissionAppService, RolePermissionAppService>();
        services.AddScoped<ITenantAppService, TenantAppService>();
        services.AddScoped<IUserAppService, UserAppService>();
        services.AddScoped<IUserRoleAppService, UserRoleAppService>();

        // Domain
        services.AddScoped<IUserRoleDomainService, UserRoleDomainService>();
        services.AddScoped<IActionDomainService, ActionDomainService>();
        services.AddScoped<IAppDomainService, AppDomainService>();
        services.AddScoped<IResourceDomainService, ResourceDomainService>();
        services.AddScoped<IRoleDomainService, RoleDomainService>();
        services.AddScoped<ITenantDomainService, TenantDomainService>();
        services.AddScoped<IUserDomainService, UserDomainService>();
        services.AddScoped<IRolePermissionDomainService, RolePermissionDomainService>();
        services.AddScoped<IJwtKeyDomainService, JwtKeyDomainService>();
        services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();

        // Infra.Data - Repositories
        services.AddScoped<IActionDataRepository, ActionDataRepository>();
        services.AddScoped<IAppDataRepository, AppDataRepository>();
        services.AddScoped<IResourceDataRepository, ResourceDataRepository>();
        services.AddScoped<IRoleDataRepository, RoleDataRepository>();
        services.AddScoped<ITenantDataRepository, TenantDataRepository>();
        services.AddScoped<IJwtKeyDataRepository, JwtKeyDataRepository>();
        services.AddScoped<IUserDataRepository, UserDataRepository>();
        services.AddScoped<IUserRoleDataRepository, UserRoleDataRepository>();
        services.AddScoped<IRolePermissionDataRepository, RolePermissionDataRepository>();
        services.AddScoped<IRefreshTokenDataRepository, RefreshTokenDataRepository>();
        services.AddScoped<IJobDefinitionDataRepository, JobDefinitionDataRepository>();

        // Infra.Messaging (Email sender no-op por enquanto)

        // Hangfire Job service
        services.AddScoped<IJobSchedulerService, HangfireJobService>();
        services.AddScoped<IJobExecutor, HangfireJobExecutor>();
        services.AddScoped<IJobSyncService, JobSyncService>();
        services.AddScoped<ScheduledSyncJobDefinitionsJob>();
        services.AddScoped<JwtKeyRotationJob>();
        services.AddHostedService<JobSyncHostedService>();

        // Data Context - Entity Framework Core
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(connectionString));


        return services;
    }
}
