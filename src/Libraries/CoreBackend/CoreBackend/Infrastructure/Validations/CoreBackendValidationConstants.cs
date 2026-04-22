using Reinforced.Typings.Attributes;

namespace CoreBackend.Infrastructure.Validations;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class CoreBackendValidationConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int UrlMaxLength = 3000;
}