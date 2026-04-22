using CoreBackend.Infrastructure.Validations;
using CoreDdd.Domain.Events;

namespace CoreBackend.Messages;

public abstract record DomainEvent : BaseMessage, IDomainEvent
{
    [NotDefault]
    public Guid RelatedCommandGuid { get; set; }
}