using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreBackend.Features.Jobs.Domain;

public abstract class BaseJobMappingOverrides : IAutoMappingOverride<Job>
{
    public virtual void Override(AutoMapping<Job> mapping)
    {
        mapping.HasMany(x => x.AffectedEntities).Not.OptimisticLock();
        mapping.HasMany(x => x.HandlingAttempts).Not.OptimisticLock();

        mapping.References(x => x.RelatedCommandJob, "RelatedCommandJobId");
    }
}