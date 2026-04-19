using ITStockM.Application.Common.Interfaces;

namespace ITStockM.Infrastructure.Services;

public sealed class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
