namespace Infrastructure.Email;

public class EmailSettings
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; } = false;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@portfolio.local";
    public string FromName { get; set; } = "Portfolio Enterprise";
    public bool EnableSsl { get; set; } = true;
}
