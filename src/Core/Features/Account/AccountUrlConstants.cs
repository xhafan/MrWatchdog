using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Account;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class AccountUrlConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string TokenVariable = "$token";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginTokenGuidVariable = "$loginTokenGuid";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ReturnUrl = "returnUrl";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginUrl = "/Account/Login";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ProviderVariable = "$provider";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountExternalLoginUrl = $"/Account/Login?provider={ProviderVariable}&handler=ExternalLogin";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountConfirmLoginUrlTemplate = $"/Account/ConfirmLogin?token={TokenVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginLinkSentUrlTemplate = $"/Account/LoginLinkSent?loginTokenGuid={LoginTokenGuidVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ApiGetLoginTokenConfirmationUrlTemplate = $"/api/Login/GetLoginTokenConfirmation?loginTokenGuid={LoginTokenGuidVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ApiCompleteLoginUrlTemplate = $"/api/CompleteLogin/CompleteLogin?loginTokenGuid={LoginTokenGuidVariable}";


    public static string WithToken(this string urlTemplate, string token)
    {
        return urlTemplate.WithVariable(TokenVariable, token);
    }

    public static string WithLoginTokenGuid(this string urlTemplate, Guid loginTokenGuid)
    {
        return urlTemplate.WithVariable(LoginTokenGuidVariable, loginTokenGuid.ToString());
    }

    public static string WithProvider(this string urlTemplate, string provider)
    {
        return urlTemplate.WithVariable(ProviderVariable, provider);
    }
}   