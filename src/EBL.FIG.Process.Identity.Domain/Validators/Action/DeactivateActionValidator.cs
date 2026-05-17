using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Domain.Validators.Action;

public class DeactivateActionValidator : AbstractValidator<ActionEntity>
{
    public DeactivateActionValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.Action.IdRequired"));

        RuleFor(x => x.IsDeleted)
            .Must(x => !x)
            .WithMessage(localization.GetMessage("Domain.Action.CannotDeactivateDeleted"));

        RuleFor(x => x.IsActive)
            .Equal(true)
            .WithMessage(localization.GetMessage("Domain.Action.AlreadyInactive"));
    }
}
