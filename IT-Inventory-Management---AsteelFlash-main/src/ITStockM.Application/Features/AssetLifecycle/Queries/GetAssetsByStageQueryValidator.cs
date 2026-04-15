using FluentValidation;

namespace ITStockM.Application.Features.AssetLifecycle.Queries;

public sealed class GetAssetsByStageQueryValidator : AbstractValidator<GetAssetsByStageQuery>
{
    public GetAssetsByStageQueryValidator()
    {
        RuleFor(x => x.Stage).NotEmpty().MaximumLength(50);
    }
}
