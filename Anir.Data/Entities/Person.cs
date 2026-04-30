using Anir.Shared.Enums;

namespace Anir.Data.Entities
{
    public class Person
    {
        public int Id { get; set; }

        // ANTES: public string? ImagenId { get; set; }
        public int? ImageFileId { get; set; }
        public StoredFile? ImageFile { get; set; }

        public string Dni { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? CellPhone { get; set; }
        public string? Email { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // CAMPOS ENUM
        // ═══════════════════════════════════════════════════════════
        public SchoolLevel? SchoolLevel { get; set; }
        public Sex? Sex { get; set; }
        public Affiliation? Affiliation { get; set; } 
        public ExecutiveRole? ExecutiveRole { get; set; }
        public Militancy? Militancy { get; set; }
        // ═══════════════════════════════════════════════════════════

        public string? Description { get; set; }
        public bool Active { get; set; } = true;

        public ICollection<AnirWorkPerson> AnirWorkPersons { get; set; } = new List<AnirWorkPerson>();
    }
}