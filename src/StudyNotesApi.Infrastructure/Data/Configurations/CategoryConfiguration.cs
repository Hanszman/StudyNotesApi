using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(category => category.Color)
            .HasMaxLength(50);

        builder.Property(category => category.CreatedAt)
            .IsRequired();

        builder.HasIndex(category => new { category.Name, category.UserId })
            .IsUnique();

        builder.HasMany(category => category.Notes)
            .WithOne(note => note.Category)
            .HasForeignKey(note => note.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
