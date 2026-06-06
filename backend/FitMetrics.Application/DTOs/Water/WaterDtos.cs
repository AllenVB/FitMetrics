namespace FitMetrics.Application.DTOs.Water;

/// <summary>Bugünkü su tüketimi ve hedefi (ml).</summary>
public record WaterTodayDto(int IntakeMl, int GoalMl);

public record AddWaterRequest(int AmountMl);
