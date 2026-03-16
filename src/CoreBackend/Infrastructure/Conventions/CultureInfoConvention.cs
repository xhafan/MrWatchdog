using System.Globalization;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace MrWatchdog.Core.Infrastructure.Conventions;

// https://stackoverflow.com/a/27945320/379279
public class CultureInfoConvention : IUserTypeConvention
{
    public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
    {
        criteria.Expect(x => x.Property.PropertyType == typeof(CultureInfo));
    }

    public void Apply(IPropertyInstance instance)
    {
        instance.CustomType("CultureInfo");
        instance.Length(10000); // the default mapping DB type is varchar(5) which is not long enough for Serbian culture info name 'sr-Latn-BA' - so forcing it to map it to 'text' DB type
    }
}