using FluentValidation;

namespace ITStockM.Application.Features.AssetLifecycle.Queries;

public sealed class GetLifecycleHistoryQueryValidator : AbstractValidator<GetLifecycleHistoryQuery>
{
    public GetLifecycleHistoryQueryValidator()
    {
        RuleFor(x => x.MaterielId).GreaterThan(0);
    }
}
