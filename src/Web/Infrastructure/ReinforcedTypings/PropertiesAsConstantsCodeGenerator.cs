using System.Reflection;
using Reinforced.Typings;
using Reinforced.Typings.Ast;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Generators;

namespace MrWatchdog.Web.Infrastructure.ReinforcedTypings;

public class PropertiesAsConstantsCodeGenerator : ClassCodeGenerator
{
    public override RtClass GenerateNode(Type element, RtClass result, TypeResolver resolver)
    {
        result = base.GenerateNode(element, result, resolver);
        result.Members.Clear();

        var properties = element
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(string))
            .OrderBy(p => p.Name);

        foreach (var prop in properties)
        {
            var field = new RtField
            {
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                Identifier = new RtIdentifier(char.ToLower(prop.Name[0]) + prop.Name[1..]),
                Type = new RtSimpleTypeName("string"),
                InitializationExpression = $"`{prop.Name}`"
            };
            result.Members.Add(field);
        }

        return result;
    }
}
