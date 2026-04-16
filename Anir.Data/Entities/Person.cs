using Anir.Shared.Enums;

namespace Anir.Data.Entities
{
    /// <summary>
    /// Entidad del dominio que representa una persona dentro del sistema.
    /// Contiene información básica de identificación, contacto y relaciones con trabajos ANIR.
    /// </summary>
    public class Person
    {
        public int Id { get; set; }

        /// <summary>
        /// Identificador de la imagen asociada (almacenada en FileStorage).
        /// </summary>
        public string? ImagenId { get; set; }

        /// <summary>
        /// Documento de identidad de la persona.
        /// </summary>
        public string Dni { get; set; } = null!;

        /// <summary>
        /// Nombre completo de la persona.
        /// </summary>
        public string FullName { get; set; } = null!;

        public string? CellPhone { get; set; }
        public string? Email { get; set; }
        public PersonAffiliation Affiliation { get; set; } = PersonAffiliation.ANIR;
        public string? Description { get; set; }

        public bool Active { get; set; } = true;

        /// <summary>
        /// Relación con los trabajos ANIR en los que participa.
        /// </summary>
        public ICollection<AnirWorkPerson> AnirWorkPersons { get; set; } = new List<AnirWorkPerson>();
    }
}
