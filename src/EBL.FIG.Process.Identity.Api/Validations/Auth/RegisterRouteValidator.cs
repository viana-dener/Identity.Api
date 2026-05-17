using EBL.FIG.Process.Identity.Application.Dto.Request.Auth;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Api.Validations.Auth;

public class RegisterRouteValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRouteValidator()
    {
        RuleFor(x => x.TenantId).GreaterThan(0).WithMessage("Application.Service.Auth.Register.InvalidTenantId");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Application.Service.Auth.Register.NameRequired");
        RuleFor(x => x.Secret).NotEmpty().WithMessage("Application.Service.Auth.Register.PasswordRequired").MinimumLength(8).WithMessage("Application.Service.Auth.Register.PasswordTooShort");
    }
}
