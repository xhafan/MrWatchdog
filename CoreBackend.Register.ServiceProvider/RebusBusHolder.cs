using Rebus.Bus;

namespace CoreBackend.Register.ServiceProvider;

/// <summary>
/// Holds the IBus instance. Registered as singleton before bus starts; populated after bus.Start().
/// Needed because ServiceProvider is immutable after build — can't add IBus registration later.
/// </summary>
public class RebusBusHolder
{
    public IBus? Bus { get; set; }
}
