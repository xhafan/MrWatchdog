using CoreDdd.Domain;

namespace MrWatchdog.Core.Infrastructure.DataProtections;

public class DataProtectionKey : Entity<string>, IAggregateRoot
{
    protected DataProtectionKey() {}

    public DataProtectionKey(string id, string xml)
    {
        Id = id;
        Xml = xml;
    }

    public virtual string Xml { get; protected set; } = null!;

    public virtual void Update(string xml)
    {
        Xml = xml;
    }
}