using CoreBackend.Account.Features.Account;
using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Account;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class AccountUrlConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginUrl = "/Account/Login";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountConfirmLoginUrlTemplate = $"/Account/ConfirmLogin?loginToken={CoreBackendAccountUrlConstants.LoginTokenVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginLinkSentUrlTemplate = $"/Account/LoginLinkSent?loginTokenGuid={CoreBackendAccountUrlConstants.LoginTokenGuidVariable}";
    

}   