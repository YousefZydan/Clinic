using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = _config["EmailSettings:Email"];
        var password = _config["EmailSettings:Password"];
        var host = _config["EmailSettings:Host"];
        int port = int.Parse(_config["EmailSettings:Port"]);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(email, password)
        };

        var mail = new MailMessage
        {
            From = new MailAddress(email, "Medora"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(to);
        
        await client.SendMailAsync(mail);
    }
}

