using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Scriban;

public interface IEmailService
{
    Task SendTemplateAsync(
        string templateCode,
        Guid tenantId,
        Dictionary<string, string> parameters,
        string toEmail
    );
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly IMessageTemplateRepository _messageTemplateRepository;
    public readonly IUnitOfWork _unitOfWork;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        IMessageTemplateRepository messageTemplateRepository,
        IUnitOfWork unitOfWork
    )
    {
        _emailSettings = emailSettings.Value;
        _messageTemplateRepository = messageTemplateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SendTemplateAsync(
        string templateCode,
        Guid tenantId,
        Dictionary<string, string> parameters,
        string toEmail
    )
    {
        var template = await _messageTemplateRepository.SingleOrDefaultAsync(mt =>
            mt.Code == templateCode && mt.TenantId == tenantId
        );

        if (template == null)
        {
            throw new NotFoundException("Không tìm thấy bản mẫu email.");
        }

        string subject = Render(template.Subject ?? "", parameters);
        string body = Render(template.BodyTemplate ?? "", parameters);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(
                _emailSettings.SmtpHost,
                _emailSettings.SmtpPort,
                _emailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto
            );

            if (!string.IsNullOrEmpty(_emailSettings.Username))
            {
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            }

            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Gửi email thất bại.", ex);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    private string Render(string templateText, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrEmpty(templateText))
            return string.Empty;

        // Thêm các biến chung
        parameters["AppName"] = _emailSettings.AppName ?? "Gym Management";
        parameters["CurrentYear"] = DateTime.UtcNow.Year.ToString();

        var template = Template.Parse(templateText);
        return template.Render(parameters);
    }
}
