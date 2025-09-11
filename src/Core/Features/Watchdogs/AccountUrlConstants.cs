using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Watchdogs;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class AccountUrlConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string TokenVariable = "$token";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginTokenGuidVariable = "$loginTokenGuid";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ReturnUrlVariable = "$returnUrl";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginUrlTemplate = $"/Account/Login?ReturnUrl={ReturnUrlVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountConfirmLoginUrlTemplate = $"/Account/ConfirmLogin?token={TokenVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginLinkSentUrlTemplate = $"/Account/LoginLinkSent?loginTokenGuid={LoginTokenGuidVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ApiGetLoginTokenConfirmationUrlTemplate = $"/api/Login/GetLoginTokenConfirmation?loginTokenGuid={LoginTokenGuidVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ApiCompleteLoginUrlTemplate = $"/api/CompleteLogin?loginTokenGuid={LoginTokenGuidVariable}";


    public static string WithToken(this string urlTemplate, string token)
    {
        return urlTemplate.WithVariable(TokenVariable, token);
    }

    public static string WithLoginTokenGuid(this string urlTemplate, Guid loginTokenGuid)
    {
        return urlTemplate.WithVariable(LoginTokenGuidVariable, loginTokenGuid.ToString());
    }
}   