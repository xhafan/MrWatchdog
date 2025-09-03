using MrWatchdog.Core.Features;
using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Jobs;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class JobUrlConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string JobGuidVariable = "$jobGuid";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string CommandJobGuidVariable = "$commandJobGuid";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string DomainEventTypeVariable = "$domainEventType";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string GetJobUrlTemplate = $"/api/Jobs/{JobGuidVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string GetRelatedDomainEventJobUrlTemplate = 
        $"/api/Jobs/GetRelatedDomainEventJob?commandJobGuid={CommandJobGuidVariable}&type={DomainEventTypeVariable}";

    public static string WithJobGuid(this string urlTemplate, Guid jobGuid)
    {
        return urlTemplate.WithVariable(JobGuidVariable, jobGuid.ToString());
    }

    public static string WithCommandJobGuid(this string urlTemplate, Guid commandJobGuid)
    {
        return urlTemplate.WithVariable(CommandJobGuidVariable, commandJobGuid.ToString());
    }

    public static string WithDomainEventType(this string urlTemplate, string domainEventType)
    {
        return urlTemplate.WithVariable(DomainEventTypeVariable, domainEventType);
    }
}