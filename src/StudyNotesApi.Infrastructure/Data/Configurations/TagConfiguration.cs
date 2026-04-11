using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(tag => tag.Id);

        builder.Property(tag => tag.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tag => tag.CreatedAt)
            .IsRequired();

        builder.HasIndex(tag => new { tag.Name, tag.UserId })
            .IsUnique();
    }
}
