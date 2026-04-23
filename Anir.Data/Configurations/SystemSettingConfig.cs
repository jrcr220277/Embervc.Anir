using Anir.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anir.Data.Configurations
{
    public class SystemSettingConfig : IEntityTypeConfiguration<SystemSetting>
    {
        public void Configure(EntityTypeBuilder<SystemSetting> builder)
        {
            builder.ToTable("SystemSettings");

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).ValueGeneratedOnAdd();

            // ── Identidad ──────────────────────────────
            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(s => s.ShortName)
                   .HasMaxLength(50);

            builder.Property(s => s.ImageFileId)
                   .IsRequired(false);

            builder.Property(s => s.TaxId)
                   .HasMaxLength(50);

            builder.Property(s => s.LegalRepresentative)
                   .HasMaxLength(150);

            builder.Property(s => s.LegalRepresentativeTitle)
                   .HasMaxLength(100);

            // ── Contacto ───────────────────────────────
            builder.Property(s => s.Address)
                   .HasMaxLength(300);

            builder.Property(s => s.Phone)
                   .HasMaxLength(30);

            builder.Property(s => s.Email)
                   .HasMaxLength(150);

            builder.Property(s => s.Website)
                   .HasMaxLength(300);

            // ── Reportes ───────────────────────────────
            builder.Property(s => s.ReportHeaderText)
                   .HasMaxLength(500);

            builder.Property(s => s.ReportFooterText)
                   .HasMaxLength(500);

            // ── Branding ───────────────────────────────
            builder.Property(s => s.PrimaryColor)
                   .HasMaxLength(7);

            // ── Meta ───────────────────────────────────
            builder.Property(s => s.LastUpdated)
                   .IsRequired();

            // ── Relación con StoredFile ────────────────
            builder.HasOne(s => s.ImageFile)
                   .WithMany()
                   .HasForeignKey(s => s.ImageFileId)
                   .OnDelete(DeleteBehavior.SetNull); // Si borran el archivo, queda null, no se borra la config
        }
    }
}