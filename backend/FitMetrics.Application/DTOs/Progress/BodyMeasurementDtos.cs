namespace FitMetrics.Application.DTOs.Progress;

public record BodyMeasurementDto(
    int Id,
    DateTime RecordedAt,
    double? WaistCm,
    double? HipCm,
    double? ChestCm,
    double? ArmCm,
    double? NeckCm,
    string? Notes);

public record CreateBodyMeasurementRequest(
    DateTime? RecordedAt,
    double? WaistCm,
    double? HipCm,
    double? ChestCm,
    double? ArmCm,
    double? NeckCm,
    string? Notes);
