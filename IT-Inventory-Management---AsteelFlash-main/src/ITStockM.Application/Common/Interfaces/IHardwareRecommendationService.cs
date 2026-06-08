using ITStockM.Application.Features.Recommendations.DTOs;
using ITStockM.Application.Common.Models;

namespace ITStockM.Services.Interfaces
{
    /// <summary>
    /// Provides intelligent recommendations for assigning hardware to employees.
    /// </summary>
    public interface IHardwareRecommendationService
    {
        Task<IReadOnlyList<HardwareRecommendationDto>> RecommendAsync(
            HardwareRecommendationRequestDto request,
            CancellationToken ct = default);

        Task RetrainAsync(CancellationToken ct = default);

        Task<AiModelStatusDto> GetModelStatusAsync(CancellationToken ct = default);
    }
}
