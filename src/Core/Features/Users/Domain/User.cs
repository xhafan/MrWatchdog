using CoreDdd.Domain;
using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Users.Domain;

public class User : VersionedEntity, IAggregateRoot
{
    protected User() {}

    public virtual string Email { get; protected set; } = null!;
    public virtual string PasswordHash { get; protected set; } = null!;
}