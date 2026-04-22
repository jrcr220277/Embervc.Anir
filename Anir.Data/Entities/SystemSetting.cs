namespace Anir.Data.Entities
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // ANTES: public string? LogoId { get; set; }
        public int? ImageFileId { get; set; }
        public StoredFile? ImageFile { get; set; }

        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
    }
}