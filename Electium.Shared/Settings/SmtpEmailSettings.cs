namespace Electium.Shared.Settings;

public class SmtpEmailSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string SecureSocketOptions { get; set; } = "StartTls";

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = "Electium";
}