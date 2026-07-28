namespace CRM.SharedKernel.Infrastructure.Configuration;

public class EmailOptions
{
    public const string SectionName = "MailServer";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string SystemUrl { get; set; } = "http://localhost:5000";
}
