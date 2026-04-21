using CoreBackend.Infrastructure.Extensions;
using Reinforced.Typings.Attributes;

namespace CoreBackend.Account.Features.Account;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class CoreBackendAccountUrlConstants
{ 
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginTokenVariable = "$loginToken";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginTokenGuidVariable = "$loginTokenGuid";

    extension(string urlTemplate)
    {
        public string WithLoginToken(string loginToken)
        {
            return urlTemplate.WithVariable(LoginTokenVariable, loginToken);
        }

        public string WithLoginTokenGuid(Guid loginTokenGuid)
        {
            return urlTemplate.WithVariable(LoginTokenGuidVariable, loginTokenGuid.ToString());
        }
    }
}