using MrWatchdog.Core.Features;
using MrWatchdog.Core.Resources;
using Reinforced.Typings.Attributes;
using Reinforced.Typings.Fluent;
using ConfigurationBuilder = Reinforced.Typings.Fluent.ConfigurationBuilder;

namespace MrWatchdog.Web.Infrastructure.ReinforcedTypings;

public static class ReinforcedTypingsConfiguration
{
    public static void Configure(ConfigurationBuilder builder)
    {
        builder.Global(global => global
            .UseModules()
            .CamelCaseForProperties()
        );

        var coreTypes = typeof(DomainConstants).Assembly.GetTypes();

        var typesAsTsInterface = coreTypes.Where(x => x.GetCustomAttributes(typeof(TsInterfaceAttribute), inherit: false).Any());

        builder.ExportAsInterfaces(typesAsTsInterface, config => config
            .WithPublicProperties()
            .AutoI(false)
            .DontIncludeToNamespace()
        );
        
        var typesAsTsClass = coreTypes.Where(x => x.GetCustomAttributes(typeof(TsClassAttribute), inherit: false).Any());
        
        builder.ExportAsClasses(typesAsTsClass, config => config
            .WithPublicProperties()
            .DontIncludeToNamespace()
        );

        var enumTypes = coreTypes.Where(x => x.IsEnum && x.IsPublic);

        builder.ExportAsEnums(enumTypes, config => config
            .DontIncludeToNamespace()
        );

        builder.ExportAsClasses([typeof(Resource)], config => config
            .WithCodeGenerator<PropertiesAsConstantsCodeGenerator>()
            .DontIncludeToNamespace()
        );
    }
}