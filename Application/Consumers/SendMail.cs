using MassTransit;
using Microsoft.Extensions.Logging;
using Application.DTOs.Services;
using Application.Interfaces;

namespace Application.Consumers;

public class SendMail : IConsumer<DTOs.Services.SendMail>
{
    private readonly ILogger<SendMail> _logger;
    private readonly IEmailSender _emailSender;
    public SendMail(ILogger<SendMail> logger, IEmailSender emailSender)
    {
        _logger = logger;
        _emailSender = emailSender;
    }

    public Task Consume(ConsumeContext<DTOs.Services.SendMail> context)
    {
        _emailSender.SendEmailAsync(context.Message.email, context.Message.subject, context.Message.htmlMessage);
        return Task.CompletedTask;
    }
}
