using EBL.FIG.Process.Identity.Application.Dto.Request.App;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Api.Validations.App;

public class CreateAppRouteValidator : AbstractValidator<CreateAppRequest>
{
    public CreateAppRouteValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Api.Validation.App.NameRequired"))
            .MaximumLength(200)
            .WithMessage(localization.GetMessage("Api.Validation.App.NameMaxLength", 200));

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Api.Validation.App.DescriptionRequired"))
            .MaximumLength(500)
            .WithMessage(localization.GetMessage("Api.Validation.App.DescriptionMaxLength", 500));
    }
}
