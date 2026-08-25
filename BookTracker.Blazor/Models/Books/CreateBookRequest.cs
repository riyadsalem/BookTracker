using System.ComponentModel.DataAnnotations;
namespace BookTracker.Blazor.Models.Books;

public sealed class CreateBookRequest
{
    [Required(ErrorMessage = "Titel is verplicht.")]
    [StringLength(100, ErrorMessage = "Titel mag maximaal 100 tekens bevatten.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Auteur is verplicht.")]
    [StringLength(100, ErrorMessage = "Auteur mag maximaal 100 tekens bevatten.")]
    public string Author { get; set; } = string.Empty;

    [Required(ErrorMessage = "Jaar is verplicht.")]
    public int Year { get; set; }
}