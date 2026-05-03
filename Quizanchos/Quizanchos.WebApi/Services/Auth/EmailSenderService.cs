using FluentEmail.Core;
using FluentEmail.Core.Models;
using Quizanchos.Common.Util;

namespace Quizanchos.WebApi.Services.Auth;

public class EmailSenderService
{
    private readonly IFluentEmail _email;
    private readonly ILogger<EmailSenderService> _logger;

    public EmailSenderService(IFluentEmail email, ILogger<EmailSenderService> logger)
    {
        _email = email;
        _logger = logger;
    }

    public Task SendEmailAsync(string recipientEmail, string subject, string content)
    {
        return SendEmailAsync(recipientEmail, subject, content, isHtml: false);
    }

    public Task SendHtmlEmailAsync(string recipientEmail, string subject, string content)
    {
        return SendEmailAsync(recipientEmail, subject, content, isHtml: true);
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string content, bool isHtml)
    {
        SendResponse sendResponse = await _email
            .To(recipientEmail)
            .Subject(subject)
            .Body(content, isHtml)
            .SendAsync();

        if (sendResponse.Successful)
        {
            _logger.LogInformation("Email sent to {Recipient} (subject: {Subject})", recipientEmail, subject);
            return;
        }

        string errorDetails = sendResponse.ErrorMessages is { Count: > 0 }
            ? string.Join("; ", sendResponse.ErrorMessages)
            : "no error details returned by sender";

        _logger.LogError(
            "Failed to send email to {Recipient} (subject: {Subject}): {Error}",
            recipientEmail,
            subject,
            errorDetails);

        throw HandledExceptionFactory.Create($"Failed to send email: {errorDetails}");
    }
}
