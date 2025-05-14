using CoreDdd.Domain;

namespace MrWatchdog.Core.Features.Shared.Domain;

/// <summary>
/// The purpose of this class is to enable NHibernate versioned concurrency model for entities derived from this class.
/// The problem this solves: 2 commands executed concurrently on the same entity might cause the result of one 
/// of the commands to be lost as the last update to win. With versioned concurrency model in place, the second update
/// would fail on update as the version value would be different, and Rebus would re-try to handle the second command again.
/// More info: https://ayende.com/blog/3946/nhibernate-mapping-concurrency .
///
/// Has Id of type long.
/// </summary>
public abstract class VersionedEntity : Entity<long>
{
    public virtual long Version { get; protected set; }
}