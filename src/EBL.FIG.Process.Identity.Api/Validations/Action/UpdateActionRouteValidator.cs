using EBL.FIG.Process.Identity.Application.Dto.Request.Action;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Api.Validations.Action;

public class UpdateActionRouteValidator : AbstractValidator<UpdateActionRequest>
{
    private readonly ILocalizationService _localization;

    public UpdateActionRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage(_localization.GetMessage("Api.Validator.Action.Update.Name.MaximumLength", 200));

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage(_localization.GetMessage("Api.Validator.Action.Update.Description.MaximumLength", 255));
    }
}
