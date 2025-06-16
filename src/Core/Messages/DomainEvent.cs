using CoreDdd.Domain.Events;
using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Messages;

public abstract record DomainEvent : BaseMessage, IDomainEvent
{
    [NotDefault]
    public Guid RelatedCommandGuid { get; set; }
}