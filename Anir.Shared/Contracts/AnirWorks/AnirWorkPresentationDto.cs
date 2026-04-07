using System.ComponentModel.DataAnnotations;

namespace Anir.Shared.Contracts.AnirWorks.Presentations;

public class AnirWorkPresentationDto
{
    public int Id { get; set; }

    [Display(Name = "Fecha de presentación")]
    public DateOnly? PresentationDate { get; set; }

    [Display(Name = "Notas")]
    public string? Notes { get; set; }
}


