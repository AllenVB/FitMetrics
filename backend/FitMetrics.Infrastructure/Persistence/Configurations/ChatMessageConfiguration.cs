using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitMetrics.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Role).IsRequired().HasMaxLength(16);
        builder.Property(x => x.Content).IsRequired(); // nvarchar(max) — uzun yanıtlar için
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}
