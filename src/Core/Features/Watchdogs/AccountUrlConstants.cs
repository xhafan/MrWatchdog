using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Watchdogs;

[TsClass(IncludeNamespace = false)]
public static class AccountUrlConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string TokenVariable = "$token";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountConfirmLoginUrl = $"/Account/ConfirmLogin?token={TokenVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginTokenGuidVariable = "$loginTokenGuid";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginLinkSentUrl = $"/Account/LoginLinkSent?loginTokenGuid={LoginTokenGuidVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ApiGetLoginTokenConfirmationUrl = $"/api/Login/GetLoginTokenConfirmation?loginTokenGuid={LoginTokenGuidVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ApiCompleteLoginUrl = $"/api/CompleteLogin?loginTokenGuid={LoginTokenGuidVariable}";
}   