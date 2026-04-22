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
        public PersonAffiliation Affiliation { get; set; } = PersonAffiliation.ANIR;
        public string? Description { get; set; }
        public bool Active { get; set; } = true;

        public ICollection<AnirWorkPerson> AnirWorkPersons { get; set; } = new List<AnirWorkPerson>();
    }
}