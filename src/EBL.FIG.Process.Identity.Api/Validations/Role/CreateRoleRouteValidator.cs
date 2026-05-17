using EBL.FIG.Process.Identity.Application.Dto.Request.Role;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Api.Validations.Role;

public class CreateRoleRouteValidator : AbstractValidator<CreateRoleRequest>
{
    private readonly ILocalizationService _localization;

    public CreateRoleRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Role.Create.Name"))
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.Role.Create.Name.MaximumLength", 100));

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage(_localization.GetMessage("Api.Validator.Role.Create.Description.MaximumLength", 255));
    }
}

