namespace FitMetrics.Application.DTOs.Knowledge;

public record KnowledgeEntryDto(int Id, string Question, string Answer, DateTime CreatedAt);

public record CreateKnowledgeEntryRequest(string Question, string Answer);
