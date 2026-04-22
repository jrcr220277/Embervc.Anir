using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations;

public class StoredFileConfig : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("StoredFiles");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedOnAdd();

        builder.Property(f => f.FileName)
               .IsRequired()
               .HasMaxLength(260);

        builder.Property(f => f.OriginalName)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(f => f.MimeType)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(f => f.SizeBytes)
               .IsRequired();

        builder.Property(f => f.Folder)
               .IsRequired()
               .HasMaxLength(300);

        builder.Property(f => f.UploadedAt)
               .IsRequired();

        builder.HasIndex(f => f.Folder);
    }
}