using Reinforced.Typings.Attributes;

namespace CoreBackend.Resources;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public class SharedTranslations
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable PropertyCanBeMadeInitOnly.Global
    public required string Ok { get; set; } = null!;
    public required string Cancel { get; set; } = null!;
    public required string Error { get; set; } = null!;
    public required string Edit { get; set; } = null!;
    public required string Save { get; set; } = null!;

    public required Dictionary<string, string> TranslationByResource { get; set; } = null!;
    // ReSharper restore PropertyCanBeMadeInitOnly.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}