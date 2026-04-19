using FluentValidation;

namespace ITStockM.Application.Features.AssetLifecycle.Commands;

public sealed class TransitionAssetStageCommandValidator : AbstractValidator<TransitionAssetStageCommand>
{
    public TransitionAssetStageCommandValidator()
    {
        RuleFor(x => x.MaterielId).GreaterThan(0);
        RuleFor(x => x.Stage).NotEmpty().MaximumLength(64);
    }
}
