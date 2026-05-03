using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Quizanchos.Common.Util;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Options;
using Quizanchos.WebApi.Services.Auth;

namespace Quizanchos.WebApi.Services;

public class ContactService
{
    private readonly QuizanchosDbContext _dbContext;
    private readonly EmailSenderService _emailSenderService;
    private readonly ContactOptions _options;
    private readonly ILogger<ContactService> _logger;

    public ContactService(
        QuizanchosDbContext dbContext,
        EmailSenderService emailSenderService,
        IOptions<ContactOptions> options,
        ILogger<ContactService> logger)
    {
        _dbContext = dbContext;
        _emailSenderService = emailSenderService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SubmitMessageAsync(ContactMessageDto dto, ClaimsPrincipal? user)
    {
        _ = dto ?? throw HandledExceptionFactory.CreateNullException(nameof(dto));

        string name = dto.Name.Trim();
        string email = dto.Email.Trim();
        string subject = dto.Subject.Trim();
        string message = dto.Message.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email)
            || string.IsNullOrEmpty(subject) || message.Length < 10)
        {
            throw HandledExceptionFactory.Create("Please fill in all fields. Message must be at least 10 characters.");
        }

        ContactMessage entity = new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Subject = subject,
            Message = message,
            CreatedAtUtc = DateTime.UtcNow,
            SubmittedByUserId = user?.FindFirstValue(ClaimTypes.NameIdentifier),
        };

        await _dbContext.ContactMessages.AddAsync(entity).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

        try
        {
            await _emailSenderService
                .SendHtmlEmailAsync(_options.RecipientEmail, $"[Contact] {entity.Subject}", BuildEmailBody(entity))
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Contact message {Id} stored but email forwarding failed.", entity.Id);
        }
    }

    private static string BuildEmailBody(ContactMessage entity)
    {
        string EncodeMultiline(string value) =>
            WebUtility.HtmlEncode(value).Replace("\n", "<br>");

        return $"""
            <p><strong>From:</strong> {WebUtility.HtmlEncode(entity.Name)} &lt;{WebUtility.HtmlEncode(entity.Email)}&gt;</p>
            <p><strong>Subject:</strong> {WebUtility.HtmlEncode(entity.Subject)}</p>
            <hr>
            <p>{EncodeMultiline(entity.Message)}</p>
            <hr>
            <p style="color:#888;font-size:12px">Submitted at {entity.CreatedAtUtc:yyyy-MM-dd HH:mm} UTC</p>
            """;
    }
}
