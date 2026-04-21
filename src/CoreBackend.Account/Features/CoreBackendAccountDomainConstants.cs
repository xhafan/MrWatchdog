using CoreBackend.Account.Features.Account.Domain;
using Reinforced.Typings.Attributes;

namespace CoreBackend.Account.Features;

[TsClass(IncludeNamespace = false)]
public static class CoreBackendAccountDomainConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginTokenEntityName = nameof(LoginToken);
}