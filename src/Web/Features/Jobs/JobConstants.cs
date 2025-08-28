using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Jobs;

[TsClass(IncludeNamespace = false)]
public static class JobConstants // todo: rename to JobUrlConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string JobGuidVariable = "$jobGuid";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string GetJobUrl = $"/api/Jobs/{JobGuidVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string CommandJobGuidVariable = "$commandJobGuid";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string DomainEventTypeVariable = "$domainEventType";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string GetRelatedDomainEventJobUrl = $"/api/Jobs/GetRelatedDomainEventJob?commandJobGuid={CommandJobGuidVariable}&type={DomainEventTypeVariable}";
}