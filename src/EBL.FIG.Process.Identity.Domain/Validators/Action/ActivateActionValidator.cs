using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace EBL.FIG.Process.Identity.Domain.Validators.Action;

public class ActivateActionValidator : AbstractValidator<ActionEntity>
{
    public ActivateActionValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.ActionEntity.IdRequired"));

        RuleFor(x => x.IsDeleted)
            .Must(x => !x)
            .WithMessage(localization.GetMessage("Domain.ActionEntity.CannotActivateDeleted"));

        RuleFor(x => x.IsActive)
            .Equal(false)
            .WithMessage(localization.GetMessage("Domain.ActionEntity.AlreadyActive"));
    }
}
