namespace Anir.Data.Entities
{
    /// <summary>
    /// Representa la relación entre un trabajo ANIR y una persona,
    /// incluyendo el porcentaje de participación de dicha persona.
    /// </summary>
    public class AnirWorkPerson
    {
        public int Id { get; set; }

        /// <summary>
        /// Identificador del trabajo ANIR asociado.
        /// </summary>
        public int AnirWorkId { get; set; }

        /// <summary>
        /// Identificador de la persona asociada.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Porcentaje de participación de la persona en el trabajo.
        /// </summary>
        public double ParticipationPercentage { get; set; }

        /// <summary>
        /// Trabajo ANIR relacionado.
        /// </summary>
        public AnirWork AnirWork { get; set; } = null!;

        /// <summary>
        /// Persona relacionada.
        /// </summary>
        public Person Person { get; set; } = null!;
    }
}
