using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Email;

public class EmailSettings
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }

    [Required]
    public string Host { get; set; } = "localhost";

    [Range(1, 65535, ErrorMessage = "Email:Port must be between 1 and 65535.")]
    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email:FromAddress must be a valid email address.")]
    public string FromAddress { get; set; } = "noreply@portfolio.local";

    public string FromName { get; set; } = "Portfolio Enterprise";
    public bool EnableSsl { get; set; } = true;
}
