using EBL.FIG.Process.Identity.Application.Dto.Request.Resource;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Api.Validations.Resource;

public class UpdateResourceRouteValidator : AbstractValidator<UpdateResourceRequest>
{
    private readonly ILocalizationService _localization;

    public UpdateResourceRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.Resource.Update.Name.MaximumLength", 100));

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage(_localization.GetMessage("Api.Validator.Resource.Update.Description.MaximumLength", 255));
    }
}
