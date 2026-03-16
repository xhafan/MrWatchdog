using CoreBackend.Features.Jobs.Domain;
using FluentNHibernate.Automapping;
using MrWatchdog.Core.Infrastructure.UserTypes;

namespace MrWatchdog.Core.Features.Jobs.Domain;

public class JobMappingOverrides : BaseJobMappingOverrides
{
    public override void Override(AutoMapping<Job> mapping)
    {
        base.Override(mapping);

        mapping.Map(x => x.InputData).CustomSqlType("jsonb").CustomType<StringAsJsonb>();
    }
}