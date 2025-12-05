using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Resources;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public class SharedTranslations
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required string Ok { get; set; } = null!;
    public required string Cancel { get; set; } = null!;
    public required string Back { get; set; } = null!;
    public required string Next { get; set; } = null!;
    public required string Finish { get; set; } = null!;
    public required string Error { get; set; } = null!;
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}