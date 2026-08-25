using System.ComponentModel.DataAnnotations;
namespace BookTracker.Blazor.Models.Auth;

public sealed class RegisterFormModel
{
    [Required(ErrorMessage = "Naam is verplicht.")]
    [StringLength(100, ErrorMessage = "Naam mag maximaal 100 tekens bevatten.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail is verplicht.")]
    [StringLength(200, ErrorMessage = "E-mail mag maximaal 200 tekens bevatten.")]
    [EmailAddress(ErrorMessage = "Voer een geldig e-mailadres in.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wachtwoord is verplicht.")]
    [MinLength(8, ErrorMessage = "Wachtwoord moet minstens 8 tekens bevatten.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bevestig je wachtwoord.")]
    [Compare(nameof(Password), ErrorMessage = "Wachtwoorden komen niet overeen.")]
    public string PasswordConfirmation { get; set; } = string.Empty;
}