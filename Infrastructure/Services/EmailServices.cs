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
        var port = int.Parse(_config["EmailSettings:Port"]);

        var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(email, password)
        };

        var mail = new MailMessage(email, to, subject, body);
        mail.IsBodyHtml = true;

        await client.SendMailAsync(mail);
    }
}
