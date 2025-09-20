namespace SlqStudio.Application.Services.EmailService;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string subject, string body);
}