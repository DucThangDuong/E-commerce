using Domain.Common;

namespace Domain.Events;

public class UserCreatedEvent : IDomainEvent
{
    public string Name { get; }
    public string Email { get; }

    public UserCreatedEvent(string name, string email)
    {
        Name = name;
        Email = email;
    }
}
