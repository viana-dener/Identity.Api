using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Domain.Validators.Resource;

public class DeactivateResourceValidator : AbstractValidator<ResourceEntity>
{
    public DeactivateResourceValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.Resource.InvalidId"));

        RuleFor(x => x.IsDeleted)
            .Equal(false)
            .WithMessage(localization.GetMessage("Domain.Resource.CannotDeactivateDeleted"));
    }
}
