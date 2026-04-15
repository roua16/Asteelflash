using FluentValidation;

namespace ITStockM.Application.Features.Predictions.Queries;

public sealed class GetHighRiskAssetsQueryValidator : AbstractValidator<GetHighRiskAssetsQuery>
{
    public GetHighRiskAssetsQueryValidator()
    {
        RuleFor(x => x.MaxScore).InclusiveBetween(0, 100);
    }
}
