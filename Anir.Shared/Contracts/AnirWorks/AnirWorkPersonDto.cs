using System.ComponentModel.DataAnnotations;

namespace Anir.Shared.Contracts.AnirWorks.Persons;

public class AnirWorkPersonDto
{
    public int Id { get; set; }

    [Required]
    public int PersonId { get; set; }

    public string? PersonName { get; set; }

    [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100.")]
    [Display(Name = "Porcentaje de participación")]
    public double ParticipationPercentage { get; set; }
}
