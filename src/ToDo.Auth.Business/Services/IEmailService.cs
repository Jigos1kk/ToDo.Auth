namespace ToDo.Auth.Business.Services;

/// <summary>
/// Отправка email-сообщений. В production заменяется реализацией,
/// работающей с SMTP-сервером или сторонним почтовым API.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}