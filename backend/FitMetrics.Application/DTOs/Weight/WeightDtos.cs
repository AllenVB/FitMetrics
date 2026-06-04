namespace FitMetrics.Application.DTOs.Weight;

public record CreateWeightEntryRequest(
    double WeightKg,
    double? BodyFatPercentage,
    DateTime? RecordedAt);

public record WeightEntryDto(
    int Id,
    double WeightKg,
    double? BodyFatPercentage,
    DateTime RecordedAt);
