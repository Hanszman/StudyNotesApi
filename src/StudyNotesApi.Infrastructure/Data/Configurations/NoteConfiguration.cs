using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Infrastructure.Data.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");

        builder.HasKey(note => note.Id);

        builder.Property(note => note.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(note => note.Content)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(note => note.CreatedAt)
            .IsRequired();

        builder.Property(note => note.IsFavorite)
            .IsRequired();

        builder.Property(note => note.IsArchived)
            .IsRequired();

        builder.Property(note => note.IsPinned)
            .IsRequired();

        builder.HasMany(note => note.NoteTags)
            .WithOne(noteTag => noteTag.Note)
            .HasForeignKey(noteTag => noteTag.NoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
