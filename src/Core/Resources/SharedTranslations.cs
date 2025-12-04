using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Resources;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public class SharedTranslations
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public string Ok { get; set; } = null!;
    public string Cancel { get; set; } = null!;
    public string Back { get; set; } = null!;
    public string Next { get; set; } = null!;
    public string Finish { get; set; } = null!;
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}