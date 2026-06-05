using FitMetrics.Application.DTOs.Reports;

namespace FitMetrics.Application.Common.Interfaces;

/// <summary>Rapor modelinden PDF üretir. Implementasyon (QuestPDF) Infrastructure'dadır.</summary>
public interface IPdfReportGenerator
{
    byte[] Generate(ReportModel model);
}
