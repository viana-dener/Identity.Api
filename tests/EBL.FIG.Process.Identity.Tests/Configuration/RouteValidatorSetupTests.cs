using EBL.FIG.Process.Identity.Api.Configuration;
using EBL.FIG.Process.Identity.Api.Validations.Action;
using EBL.FIG.Process.Identity.Api.Validations.Auth;
using EBL.FIG.Process.Identity.Api.Validations.Resource;
using EBL.FIG.Process.Identity.Api.Validations.Role;
using EBL.FIG.Process.Identity.Api.Validations.Tenant;
using EBL.FIG.Process.Identity.Api.Validations.User;
using EBL.FIG.Process.Identity.Application.Dto.Request.Action;
using EBL.FIG.Process.Identity.Application.Dto.Request.Auth;
using EBL.FIG.Process.Identity.Application.Dto.Request.Resource;
using EBL.FIG.Process.Identity.Application.Dto.Request.Role;
using EBL.FIG.Process.Identity.Application.Dto.Request.Tenant;
using EBL.FIG.Process.Identity.Application.Dto.Request.User;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EBL.FIG.Process.Identity.Tests.Configuration;

public class RouteValidatorSetupTests
{
    private static IServiceProvider BuildProviderWithDependencies()
    {
        var services = new ServiceCollection();
        services.AddRouteValidatorSetup();
        var locMock = new Mock<ILocalizationService>();
        locMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("mensagem");
        locMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("mensagem");
        services.AddScoped(_ => locMock.Object);
        return services.BuildServiceProvider();
    }

    [Fact(DisplayName = "AddRouteValidatorSetup - Deve registrar todos os validadores esperados")]
    [Trait("Api", "")]
    public void AddRouteValidatorSetup_Chamado_RegistraTodosValidadores()
    {
        var provider = BuildProviderWithDependencies();

        Assert.NotNull(provider.GetService<IValidator<CreateActionRequest>>());
        Assert.NotNull(provider.GetService<IValidator<UpdateActionRequest>>());
        Assert.NotNull(provider.GetService<IValidator<CreateResourceRequest>>());
        Assert.NotNull(provider.GetService<IValidator<UpdateResourceRequest>>());
        Assert.NotNull(provider.GetService<IValidator<CreateRoleRequest>>());
        Assert.NotNull(provider.GetService<IValidator<UpdateRoleRequest>>());
        Assert.NotNull(provider.GetService<IValidator<CreateTenantRequest>>());
        Assert.NotNull(provider.GetService<IValidator<UpdateTenantRequest>>());
        Assert.NotNull(provider.GetService<IValidator<CreateUserRequest>>());
        Assert.NotNull(provider.GetService<IValidator<UpdateUserRequest>>());
        Assert.NotNull(provider.GetService<IValidator<UpdatePasswordRequest>>());
        Assert.NotNull(provider.GetService<IValidator<RegisterRequest>>());
        Assert.NotNull(provider.GetService<IValidator<LoginRequest>>());
        Assert.NotNull(provider.GetService<IValidator<RefreshRequest>>());
        Assert.NotNull(provider.GetService<IValidator<ForgotPasswordRequest>>());
        Assert.NotNull(provider.GetService<IValidator<ValidateResetTokenRequest>>());
        Assert.NotNull(provider.GetService<IValidator<ResetPasswordRequest>>());
    }

    [Fact(DisplayName = "AddRouteValidatorSetup - Deve registrar IHttpContextAccessor como Singleton")]
    [Trait("Api", "")]
    public void AddRouteValidatorSetup_Chamado_RegistraHttpContextAccessor()
    {
        var services = new ServiceCollection();
        services.AddRouteValidatorSetup();
        var provider = services.BuildServiceProvider();

        var accessor = provider.GetService<IHttpContextAccessor>();

        Assert.NotNull(accessor);
    }

    [Fact(DisplayName = "AddRouteValidatorSetup - Deve retornar a própria instância de IServiceCollection")]
    [Trait("Api", "")]
    public void AddRouteValidatorSetup_Chamado_RetornaServicesEncadeamento()
    {
        var services = new ServiceCollection();
        var result = services.AddRouteValidatorSetup();

        Assert.Same(services, result);
    }

    [Fact(DisplayName = "AddRouteValidatorSetup - Validadores devem ser registrados como Scoped")]
    [Trait("Api", "")]
    public void AddRouteValidatorSetup_Chamado_ValidadoresComoScoped()
    {
        var services = new ServiceCollection();
        services.AddRouteValidatorSetup();

        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IValidator<CreateActionRequest>));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}
