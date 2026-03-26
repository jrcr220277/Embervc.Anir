namespace Anir.Data.Entities
{
    /// <summary>
    /// Representa una presentación asociada a un trabajo ANIR,
    /// incluyendo la fecha en que se realizó y notas adicionales.
    /// </summary>
    public class AnirWorkPresentation
    {
        public int Id { get; set; }

        /// <summary>
        /// Identificador del trabajo ANIR al que pertenece esta presentación.
        /// </summary>
        public int AnirWorkId { get; set; }

        /// <summary>
        /// Fecha en que se realizó la presentación.
        /// </summary>
        public DateOnly? PresentationDate { get; set; }

        /// <summary>
        /// Notas o comentarios adicionales sobre la presentación.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Trabajo ANIR relacionado.
        /// </summary>
        public AnirWork AnirWork { get; set; } = null!;
    }
}
