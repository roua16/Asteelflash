using ITStockM.Application.Features.Maintenance.DTOs;
using ITStockM.Application.Common.Models;

namespace ITStockM.Services.Interfaces
{
    /// <summary>
    /// Predicts maintenance ticket priority from textual and operational signals.
    /// </summary>
    public interface ITicketPrioritizationService
    {
        Task<TicketPriorityPredictionDto> PredictPriorityAsync(
            TicketPriorityRequestDto request,
            CancellationToken ct = default);

        Task RetrainAsync(CancellationToken ct = default);

        Task<AiModelStatusDto> GetModelStatusAsync(CancellationToken ct = default);
    }
}
