using Application.DTOs.Services;
using Domain.Events;
using MassTransit;
using MediatR;

namespace Application.Features.Customers.Events;

public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public UserCreatedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        var orderEvent = new SendMail(
            notification.Email, 
            "Chào mừng bạn đến với Food Delivery App!",
            $"Xin chào {notification.Name}!\nCảm ơn bạn đã đăng ký tài khoản tại Food Delivery App. Chúng tôi rất vui được phục vụ bạn!");
        
        await _publishEndpoint.Publish(orderEvent, cancellationToken);
    }
}
