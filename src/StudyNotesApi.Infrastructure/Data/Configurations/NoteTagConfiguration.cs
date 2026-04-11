using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Infrastructure.Data.Configurations;

public class NoteTagConfiguration : IEntityTypeConfiguration<NoteTag>
{
    public void Configure(EntityTypeBuilder<NoteTag> builder)
    {
        builder.ToTable("NoteTags");

        builder.HasKey(noteTag => new { noteTag.NoteId, noteTag.TagId });

        builder.HasOne(noteTag => noteTag.Tag)
            .WithMany(tag => tag.NoteTags)
            .HasForeignKey(noteTag => noteTag.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
