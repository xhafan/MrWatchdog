using CoreUtils;
using Rebus.Bus;
using Rebus.Pipeline;
using Rebus.Transport;
using System.Reflection;

namespace MrWatchdog.Core.Infrastructure.Rebus;

// Rebus does not expose the handling queue name, so we have to resort to reflection to get it. Refactor if Rebus adds public API for this.
public class RebusHandlingQueueGetter : IRebusHandlingQueueGetter
{
    public string GetHandlingQueue()
    {
        Guard.Hope(MessageContext.Current != null, "Rebus message context is not set.");

        var owningBus = (RebusBus) ((ITransactionContextWithOwningBus) ((MessageContext) MessageContext.Current).TransactionContext).OwningBus;
        Guard.Hope(owningBus != null, nameof(owningBus) + " is null");
        var transport = _GetInstanceField(typeof(RebusBus), owningBus, "_transport") as ITransport;
        Guard.Hope(transport != null, nameof(transport) + " is null");
        return transport.Address;
    }

    // taken from https://stackoverflow.com/a/3303182/379279
    private object? _GetInstanceField(Type type, object instance, string fieldName)
    {
        var bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        var field = type.GetField(fieldName, bindFlags);
        Guard.Hope(field != null, nameof(field) + " is null");
        return field.GetValue(instance);
    }
}