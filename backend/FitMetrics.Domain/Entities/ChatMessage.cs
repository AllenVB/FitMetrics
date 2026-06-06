using FitMetrics.Domain.Common;

namespace FitMetrics.Domain.Entities;

/// <summary>
/// AI Asistan sohbetinde kalıcı tek bir mesaj. Role: "user" | "assistant".
/// CreatedAt sıralama için kullanılır.
/// </summary>
public class ChatMessage : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}
