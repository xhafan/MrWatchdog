namespace CoreBackend.Infrastructure.Rebus;

/// Holds ambient data shared across an async flow.
/// The instance is intentionally mutable so changes are visible across awaits.
public class FlowContext<TData>
{
    public TData? Value;
}