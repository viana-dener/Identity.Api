using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Domain.Validators.Resource;

public class CreateResourceValidator : AbstractValidator<ResourceEntity>
{
    public CreateResourceValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.Resource.NameRequired"))
            .MaximumLength(100)
            .WithMessage(localization.GetMessage("Domain.Resource.NameMaxLength", 100));

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.Resource.DescriptionRequired"))
            .MaximumLength(255)
            .WithMessage(localization.GetMessage("Domain.Resource.DescriptionMaxLength", 255));
    }
}
