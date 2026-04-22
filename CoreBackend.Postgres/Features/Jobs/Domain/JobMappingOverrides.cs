using CoreBackend.Features.Jobs.Domain;
using CoreBackend.Postgres.Infrastructure.UserTypes;
using FluentNHibernate.Automapping;

namespace CoreBackend.Postgres.Features.Jobs.Domain;

public class JobMappingOverrides : BaseJobMappingOverrides
{
    public override void Override(AutoMapping<Job> mapping)
    {
        base.Override(mapping);

        mapping.Map(x => x.InputData).CustomSqlType("jsonb").CustomType<StringAsJsonb>();
    }
}