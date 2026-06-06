using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitMetrics.Infrastructure.Persistence.Configurations;

public class KnowledgeEntryConfiguration : IEntityTypeConfiguration<KnowledgeEntry>
{
    public void Configure(EntityTypeBuilder<KnowledgeEntry> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Question).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Answer).IsRequired().HasMaxLength(4000);
    }
}
