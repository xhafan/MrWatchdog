using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using MrWatchdog.Core.Infrastructure.UserTypes;

namespace MrWatchdog.Core.Features.Jobs.Domain;

public class JobMappingOverrides : IAutoMappingOverride<Job>
{
    public void Override(AutoMapping<Job> mapping)
    {
        mapping.HasMany(x => x.AffectedEntities).Not.OptimisticLock();
        mapping.HasMany(x => x.HandlingAttempts).Not.OptimisticLock();

        mapping.Map(x => x.InputData).CustomSqlType("jsonb").CustomType<StringAsJsonb>();
    }
}