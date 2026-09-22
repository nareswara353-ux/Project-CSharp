using System.ComponentModel.DataAnnotations;

namespace Application.Common;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "Jwt:Secret is required.")]
    [MinLength(32, ErrorMessage = "Jwt:Secret must be at least 32 characters.")]
    public string Secret { get; set; } = string.Empty;

    [Required(ErrorMessage = "Jwt:Issuer is required.")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "Jwt:Audience is required.")]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "Jwt:ExpirationMinutes must be between 1 and 1440.")]
    public int ExpirationMinutes { get; set; } = 60;
}
