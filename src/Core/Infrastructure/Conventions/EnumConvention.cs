using CoreUtils;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using NHibernate.Type;

namespace MrWatchdog.Core.Infrastructure.Conventions;

public class EnumConvention : IUserTypeConvention
{
    public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
    {
        criteria.Expect(x =>
        {
            var propertyType = x.Property.PropertyType;
            if (propertyType.IsEnum) return true;

            var nullableUnderlyingType = Nullable.GetUnderlyingType(propertyType);
            return nullableUnderlyingType != null && nullableUnderlyingType.IsEnum;
        });
    }

    public void Apply(IPropertyInstance instance)
    {
        var isNullable = false;
        var propertyType = instance.Property.PropertyType;
        if (!propertyType.IsEnum)
        {
            propertyType = Nullable.GetUnderlyingType(propertyType);
            Guard.Hope(propertyType != null, nameof(propertyType) + " is null");
            isNullable = true;
        }

        var enumToCharType = typeof(EnumCharType<>).MakeGenericType(propertyType);
        instance.CustomType(enumToCharType);

        if (isNullable)
        {
            instance.Nullable();
        }
    }
}