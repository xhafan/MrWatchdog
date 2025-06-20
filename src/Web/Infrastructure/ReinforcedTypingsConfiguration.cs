﻿using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using Reinforced.Typings.Attributes;
using Reinforced.Typings.Fluent;
using ConfigurationBuilder = Reinforced.Typings.Fluent.ConfigurationBuilder;

namespace MrWatchdog.Web.Infrastructure;

public static class ReinforcedTypingsConfiguration
{
    public static void Configure(ConfigurationBuilder builder)
    {
        builder.Global(global => global
            .UseModules()
            .CamelCaseForProperties()
        );

        var typesAsTsInterface = typeof(JobDto).Assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(TsInterfaceAttribute), inherit: false).Any());

        builder.ExportAsInterfaces(typesAsTsInterface, config => config
            .WithPublicProperties()
            .AutoI(false)
            .DontIncludeToNamespace()
        );
        
        var typesAsTsClass = typeof(RebusConstants).Assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(TsClassAttribute), inherit: false).Any());
        
        builder.ExportAsClasses(typesAsTsClass, config => config
            .WithPublicProperties()
            .DontIncludeToNamespace()
        );

        var enumTypes = typeof(JobKind).Assembly.GetTypes().Where(x => x.IsEnum && x.IsPublic).ToList();

        builder.ExportAsEnums(enumTypes, config => config
            .DontIncludeToNamespace()
        );        
    }
}