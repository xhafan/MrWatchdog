using CoreBackend.Features.Account.Domain;
using Reinforced.Typings.Attributes;

namespace CoreBackend.Features;

[TsClass(IncludeNamespace = false)]
public static class CoreBackendDomainConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginTokenEntityName = nameof(LoginToken);
}