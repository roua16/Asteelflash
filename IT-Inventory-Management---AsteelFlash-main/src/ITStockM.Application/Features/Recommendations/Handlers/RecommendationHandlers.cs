using ITStockM.Application.Features.Recommendations.DTOs;
using ITStockM.Application.Features.Recommendations.Queries;
using ITStockM.Services.Interfaces;
using MediatR;

namespace ITStockM.Application.Features.Recommendations.Handlers;

public sealed class GetHardwareRecommendationsHandler : IRequestHandler<GetHardwareRecommendationsQuery, IReadOnlyList<HardwareRecommendationDto>>
{
    private readonly IHardwareRecommendationService _service;

    public GetHardwareRecommendationsHandler(IHardwareRecommendationService service)
    {
        _service = service;
    }

    public Task<IReadOnlyList<HardwareRecommendationDto>> Handle(GetHardwareRecommendationsQuery request, CancellationToken cancellationToken)
        => _service.RecommendAsync(request.Request, cancellationToken);
}
