using EBL.FIG.Process.Identity.Application.Dto.Request.Auth;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Api.Validations.Auth;

public class ValidateResetTokenRouteValidator : AbstractValidator<ValidateResetTokenRequest>
{
    public ValidateResetTokenRouteValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Application.Service.Auth.ValidateResetToken.TokenRequired");
    }
}
