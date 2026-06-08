using ITStockM.Application.Features.Recommendations.DTOs;
using MediatR;

namespace ITStockM.Application.Features.Recommendations.Queries;

public sealed record GetHardwareRecommendationsQuery(HardwareRecommendationRequestDto Request) : IRequest<IReadOnlyList<HardwareRecommendationDto>>;
