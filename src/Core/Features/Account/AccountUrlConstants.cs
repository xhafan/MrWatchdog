using CoreBackend.Infrastructure.Extensions;
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
    public const string AccountConfirmLoginUrlTemplate = $"/Account/ConfirmLogin?token={TokenVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginLinkSentUrlTemplate = $"/Account/LoginLinkSent?loginTokenGuid={LoginTokenGuidVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ApiGetLoginTokenConfirmationUrlTemplate = $"/api/Login/GetLoginTokenConfirmation?loginTokenGuid={LoginTokenGuidVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ApiCompleteLoginUrlTemplate = $"/api/CompleteLogin/CompleteLogin?loginTokenGuid={LoginTokenGuidVariable}";


    extension(string urlTemplate)
    {
        public string WithToken(string token)
        {
            return urlTemplate.WithVariable(TokenVariable, token);
        }

        public string WithLoginTokenGuid(Guid loginTokenGuid)
        {
            return urlTemplate.WithVariable(LoginTokenGuidVariable, loginTokenGuid.ToString());
        }
    }
}   