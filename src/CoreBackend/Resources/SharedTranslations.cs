using Reinforced.Typings.Attributes;

namespace CoreBackend.Resources;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public class SharedTranslations // todo: refactor it to a Dictionary<string, string> to make it more flexible?
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required string Ok { get; set; } = null!;
    public required string Cancel { get; set; } = null!;
    public required string Back { get; set; } = null!;
    public required string Next { get; set; } = null!;
    public required string Finish { get; set; } = null!;
    public required string Error { get; set; } = null!;
    public required string Edit { get; set; } = null!;
    public required string Save { get; set; } = null!;
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}