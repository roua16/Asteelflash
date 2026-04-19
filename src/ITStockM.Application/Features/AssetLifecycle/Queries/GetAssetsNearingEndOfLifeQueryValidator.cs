using FluentValidation;

namespace ITStockM.Application.Features.AssetLifecycle.Queries;

public sealed class GetAssetsNearingEndOfLifeQueryValidator : AbstractValidator<GetAssetsNearingEndOfLifeQuery>
{
    public GetAssetsNearingEndOfLifeQueryValidator()
    {
        RuleFor(x => x.WithinMonths).InclusiveBetween(1, 120);
    }
}
