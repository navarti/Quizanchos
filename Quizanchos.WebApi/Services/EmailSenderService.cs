using FluentEmail.Core;
using FluentEmail.Core.Models;

namespace Quizanchos.WebApi.Services;

public class EmailSenderService
{
    private readonly IFluentEmail _email;

    public EmailSenderService(IFluentEmail email)
    {
        _email = email;
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
            .Body(content)
            .SendAsync();
    }
}
