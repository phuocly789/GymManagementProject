public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string FromEmail { get; set; }
    public string FromName { get; set; } = "Gym Management";
    public string Username { get; set; }
    public string Password { get; set; }
    public string AppName { get; set; } = "Gym Management";
}
