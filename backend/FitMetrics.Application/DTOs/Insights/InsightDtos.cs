namespace FitMetrics.Application.DTOs.Insights;

public enum InsightSeverity
{
    Positive = 1,
    Info = 2,
    Warning = 3
}

public enum InsightCategory
{
    Calories = 1,
    Protein = 2,
    Macros = 3,
    Workout = 4,
    Weight = 5,
    Consistency = 6
}

public record InsightDto(
    InsightCategory Category,
    InsightSeverity Severity,
    string Title,
    string Message,
    string? Metric);

public record InsightsResponse(
    DateTime GeneratedAt,
    int DaysAnalyzed,
    List<InsightDto> Insights);
