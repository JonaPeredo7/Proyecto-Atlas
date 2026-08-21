using System.ComponentModel.DataAnnotations;

namespace Gimnasio.Api.Contracts.Auth;

public sealed class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;

    [Required, MaxLength(80)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; init; } = string.Empty;
}
