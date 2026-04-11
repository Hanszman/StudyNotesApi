using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.HasMany(user => user.Categories)
            .WithOne(category => category.User)
            .HasForeignKey(category => category.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Tags)
            .WithOne(tag => tag.User)
            .HasForeignKey(tag => tag.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Notes)
            .WithOne(note => note.User)
            .HasForeignKey(note => note.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
