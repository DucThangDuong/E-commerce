using MassTransit;
using Microsoft.Extensions.Logging;
using Application.DTOs.Services;
using Application.IServices;

namespace Application.Consumers;

public class SendMailConsumer : IConsumer<SendMail>
{
    private readonly ILogger<SendMail> _logger;
    private readonly IEmailSender _emailSender;
    public SendMailConsumer(ILogger<SendMail> logger, IEmailSender emailSender)
    {
        _logger = logger;
        _emailSender = emailSender;
    }

    public Task Consume(ConsumeContext<SendMail> context)
    {
        _emailSender.SendEmailAsync(context.Message.email, context.Message.subject, context.Message.htmlMessage);
        return Task.CompletedTask;
    }
}
