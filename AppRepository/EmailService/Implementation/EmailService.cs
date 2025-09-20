using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SlqStudio.Application.Services.EmailService.Models;

namespace SlqStudio.Application.Services.EmailService.Implementation;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task<bool> SendEmailAsync(string subject, string body)
    {
        try
        {
            var fromAddress = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName);
            var toAddress = new MailAddress(_smtpSettings.RecipientEmail);
            
            var htmlBody = System.Net.WebUtility.HtmlEncode(body)
                .Replace("\r\n", "<br>")
                .Replace("\n", "<br>");

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            using var smtp = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.SenderEmail, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            await smtp.SendMailAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

}