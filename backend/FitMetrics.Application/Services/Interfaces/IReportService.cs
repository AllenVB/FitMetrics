namespace FitMetrics.Application.Services.Interfaces;

public interface IReportService
{
    /// <summary>Belirtilen ay için kullanıcının ilerleme raporunu PDF olarak üretir.</summary>
    Task<byte[]> GenerateMonthlyReportAsync(int userId, int year, int month, CancellationToken ct = default);
}
