using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using NHibernate.Type;

namespace MrWatchdog.Core.Infrastructure.Conventions;

public class DateTimeConvention : IPropertyConvention
{
    public void Apply(IPropertyInstance instance)
    {
        if (instance.Type == typeof(DateTime) || instance.Type == typeof(DateTime?))
        {
            instance.CustomType<UtcDateTimeType>();
        }
    }
}