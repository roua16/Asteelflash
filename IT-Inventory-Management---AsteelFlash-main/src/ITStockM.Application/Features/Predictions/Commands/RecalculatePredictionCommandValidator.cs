using FluentValidation;

namespace ITStockM.Application.Features.Predictions.Commands;

public sealed class RecalculatePredictionCommandValidator : AbstractValidator<RecalculatePredictionCommand>
{
    public RecalculatePredictionCommandValidator()
    {
        RuleFor(x => x.MaterielId).GreaterThan(0);
    }
}
