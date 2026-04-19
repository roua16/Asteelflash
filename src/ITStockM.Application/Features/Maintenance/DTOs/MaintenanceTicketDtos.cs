namespace ITStockM.Application.Features.Maintenance.DTOs;

public sealed record MaintenanceTicketDto(
    int Id,
    int MaterielId,
    string ProblemDescription,
    int? ReportedByEmployeeId,
    string Status,
    decimal? Cost,
    DateTime ReportedAt,
    DateTime? ResolvedAt,
    string? Resolution);

public sealed record CreateMaintenanceTicketDto(
    int MaterielId,
    string ProblemDescription,
    int? ReportedByEmployeeId);

public sealed record UpdateMaintenanceTicketDto(
    string ProblemDescription,
    int? ReportedByEmployeeId,
    string Status,
    decimal? Cost,
    DateTime? ResolvedAt,
    string? Resolution);
