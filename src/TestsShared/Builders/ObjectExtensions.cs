using System.Linq.Expressions;
using System.Reflection;
using CoreUtils;

namespace MrWatchdog.TestsShared.Builders;

// This class is here mainly to set entity Ids when used in a unit test where NHibernate would not magically set the entity Id.
// Don't abuse setting private properties unnecessarily. Use a proper way to construct an object and use its public
// properties and methods to set the required state.
public static class ObjectExtensions
{
    public static void SetPrivateProperty<T>(this T target, Expression<Func<T, object>> property, object propertyValue) where T : notnull 
    {
        SetPrivateProperty(target, GetPropertyName(property), propertyValue);
    }

    public static void SetPrivateProperty(this object obj, string propertyName, object value)
    {
        var propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Guard.Hope(propertyInfo != null, $"{nameof(propertyInfo)} is null for property {propertyName}");
        propertyInfo.SetValue(obj, value, null);
    }
    
    public static object? GetPrivateProperty(this object obj, string propertyName)
    {
        var propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Guard.Hope(propertyInfo != null, $"{nameof(propertyInfo)} is null for property {propertyName}");
        return propertyInfo.GetValue(obj);
    } 

    // http://beta.blogs.microsoft.co.il/blogs/smallfish/archive/2009/04/02/how-to-get-a-property-name-using-lambda-expression-in-c-3-0.aspx
    public static string GetPropertyName<T>(Expression<Func<T, object>> property)
    {
        return GetMemberName(property.Body);
    }

    public static string GetMemberName(Expression expression)
    {
        var memberExpression = expression as MemberExpression;
        if (memberExpression?.Expression != null)
        {
            if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                return GetMemberName(memberExpression.Expression)
                       + "."
                       + memberExpression.Member.Name;
            }
            return memberExpression.Member.Name;
        }

        if (expression is UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType != ExpressionType.Convert)
            {
                throw new Exception($"Cannot interpret member from {expression}");
            }

            return GetMemberName(unaryExpression.Operand);
        }
        throw new Exception($"Could not determine member from {expression}");
    }
}
