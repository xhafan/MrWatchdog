using CoreDdd.Domain;
using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Account.Domain;

public class User : VersionedEntity, IAggregateRoot
{
    protected User() {}

    public User(string email)
    {
        Email = email;
    }

    public virtual string Email { get; protected set; } = null!;

    public virtual UserDto GetDto()
    {
        return new UserDto(Id, Email);
    }
}