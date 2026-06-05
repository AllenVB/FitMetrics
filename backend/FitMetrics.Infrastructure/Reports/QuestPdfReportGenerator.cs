using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FitMetrics.Infrastructure.Reports;

public class QuestPdfReportGenerator : IPdfReportGenerator
{
    private const string Brand = "#059669";
    private const string BrandLight = "#d1fae5";
    private const string Dark = "#0f172a";
    private const string Grey = "#64748b";
    private const string Line = "#e2e8f0";

    static QuestPdfReportGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(ReportModel model)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Dark));

                page.Header().Element(h => ComposeHeader(h, model));
                page.Content().PaddingVertical(16).Element(c => ComposeContent(c, model));

                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(9).FontColor(Grey));
                    text.Span($"FitMetrics • {model.GeneratedAt:dd.MM.yyyy HH:mm} • Sayfa ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, ReportModel m)
    {
        container.Background(Brand).Padding(16).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("FitMetrics").FontSize(22).Bold().FontColor("#ffffff");
                col.Item().Text("Aylık İlerleme Raporu").FontSize(12).FontColor(BrandLight);
            });
            row.ConstantItem(170).Column(col =>
            {
                col.Item().AlignRight().Text(m.Period).FontSize(14).Bold().FontColor("#ffffff");
                col.Item().AlignRight().Text(m.FullName).FontSize(11).FontColor(BrandLight);
            });
        });
    }

    private static void ComposeContent(IContainer container, ReportModel m)
    {
        container.Column(col =>
        {
            col.Spacing(12);

            col.Item().Text(t =>
            {
                t.Span("Hedef: ").SemiBold();
                t.Span(m.Goal);
            });

            Section(col, "Beslenme");
            StatRow(col, "Kayıt girilen gün", $"{m.LoggedDays} gün");
            StatRow(col, "Günlük ortalama kalori", $"{m.AvgCalories:0} / {m.CalorieGoal} kcal");
            StatRow(col, "Günlük ortalama protein", $"{m.AvgProtein:0} / {m.ProteinGoal} g");

            Section(col, "Antrenman");
            StatRow(col, "Toplam antrenman", $"{m.WorkoutCount}");
            StatRow(col, "Yakılan kalori", $"{m.CaloriesBurned:0} kcal");
            col.Item().PaddingTop(4).Element(c => MuscleTable(c, m));

            Section(col, "Kilo & Vücut");
            StatRow(col, "Dönem başı kilo", m.StartWeightKg.HasValue ? $"{m.StartWeightKg.Value} kg" : "-");
            StatRow(col, "Güncel kilo", $"{m.CurrentWeightKg} kg");
            StatRow(col, "Değişim", m.WeightChangeKg.HasValue ? $"{(m.WeightChangeKg.Value >= 0 ? "+" : "")}{m.WeightChangeKg.Value} kg" : "-");
            StatRow(col, "BMI", $"{m.Bmi}");
        });
    }

    private static void Section(ColumnDescriptor col, string title)
    {
        col.Item().PaddingTop(6).BorderBottom(1).BorderColor(Line).PaddingBottom(4)
            .Text(title).FontSize(14).Bold().FontColor(Brand);
    }

    private static void StatRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().Text(label).FontColor(Grey);
            row.ConstantItem(190).AlignRight().Text(value).SemiBold();
        });
    }

    private static void MuscleTable(IContainer container, ReportModel m)
    {
        if (m.MuscleBreakdown.Count == 0)
        {
            container.Text("Bu ay kuvvet antrenmanı kaydı yok.").Italic().FontColor(Grey);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn();
                c.ConstantColumn(110);
            });

            table.Header(header =>
            {
                header.Cell().BorderBottom(1).BorderColor(Line).PaddingBottom(3).Text("Kas Grubu").SemiBold();
                header.Cell().BorderBottom(1).BorderColor(Line).PaddingBottom(3).AlignRight().Text("Antrenman").SemiBold();
            });

            foreach (var s in m.MuscleBreakdown)
            {
                table.Cell().PaddingVertical(3).Text(s.Muscle);
                table.Cell().PaddingVertical(3).AlignRight().Text($"{s.Count}");
            }
        });
    }
}
