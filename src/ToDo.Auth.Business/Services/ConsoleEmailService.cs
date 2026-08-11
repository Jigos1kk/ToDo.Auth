using Microsoft.Extensions.Logging;

namespace ToDo.Auth.Business.Services;

/// <summary>
/// Заглушка email-сервиса: выводит содержимое письма в журнал.
/// В production заменяется на реальную SMTP-реализацию.
/// </summary>
public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "===== EMAIL =====\nКому: {To}\nТема: {Subject}\n---\n{Body}\n==================",
            to, subject, body);

        return Task.CompletedTask;
    }
}