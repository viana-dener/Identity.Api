using EBL.FIG.Process.Identity.Application.Dto.Request.Role;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Api.Validations.Role;

public class UpdateRoleRouteValidator : AbstractValidator<UpdateRoleRequest>
{
    private readonly ILocalizationService _localization;

    public UpdateRoleRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.Role.Update.Name.MaximumLength", 100));

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage(_localization.GetMessage("Api.Validator.Role.Update.Description.MaximumLength", 255));
    }
}
