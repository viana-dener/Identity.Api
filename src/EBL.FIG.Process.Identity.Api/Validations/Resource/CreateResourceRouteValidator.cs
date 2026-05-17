using EBL.FIG.Process.Identity.Application.Dto.Request.Resource;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Api.Validations.Resource;

public class CreateResourceRouteValidator : AbstractValidator<CreateResourceRequest>
{
    private readonly ILocalizationService _localization;

    public CreateResourceRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.AppId)
            .GreaterThan(0).WithMessage(_localization.GetMessage("Api.Validator.Resource.Create.AppId"));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Resource.Create.Name"))
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.Resource.Create.Name.MaximumLength", 100));

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage(_localization.GetMessage("Api.Validator.Resource.Create.Description.MaximumLength", 255));
    }
}
