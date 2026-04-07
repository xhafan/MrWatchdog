using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreBackend.Features.Jobs.Domain;

public abstract class BaseJobMappingOverrides : IAutoMappingOverride<Job>
{
    public virtual void Override(AutoMapping<Job> mapping)
    {
        mapping.HasMany(x => x.AffectedEntities).Not.OptimisticLock(); 
        // Optimistic lock disabled is needed so concurrent Job sql updates are not issued when
        // concurrently adding many AffectedEntities;
        // See https://nhibernate.info/doc/nh/en/index.html#collections: (14) optimistic-lock (optional - defaults to true):
        // specifies that changes to the state of the collection results in increment of the owning entity's version.
        // More info here https://stackoverflow.com/a/11159419/379279

        mapping.HasMany(x => x.HandlingAttempts).Not.OptimisticLock();

        mapping.References(x => x.RelatedCommandJob, "RelatedCommandJobId");
    }
}