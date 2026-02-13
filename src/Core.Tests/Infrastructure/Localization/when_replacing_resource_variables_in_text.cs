using System.Globalization;
using MrWatchdog.Core.Infrastructure.Localization;

namespace MrWatchdog.Core.Tests.Infrastructure.Localization;

[TestFixture]
public class when_replacing_resource_variables_in_text
{
    [TestCase(
        "Choose '${Resource_CreateWatchdog}' to receive email notification.",
        "en",
        "Choose 'Create watchdog' to receive email notification.", TestName = "1 English text with resource variable")]
    [TestCase(
        "Vyberte '${Resource_CreateWatchdog}' abyste dostávali emailová oznámení.",
        "cs",
        "Vyberte 'Vytvořit hlídacího psa' abyste dostávali emailová oznámení.", TestName = "2 Czech text with resource variable")]
    [TestCase(
        "Choose 'Create watchdog' to receive email notification.",
        "en",
        "Choose 'Create watchdog' to receive email notification.", TestName = "3 text without resource variables")]
    [TestCase("", "cs", "", TestName = "4 empty text")]
    [TestCase(null, "cs", null, TestName = "5 null text")]
    [TestCase(
        "Choose '${Resource_CreateWatchdog}' to receive email notification.",
        "az",
        "Choose 'Create watchdog' to receive email notification.", TestName = "6 text with resource variable without the resource translation")]
    public void localized_text_is_resolved_correctly(
        string? textWithResourceVariables, 
        string cultureName, 
        string expectedTextWithReplacedResourceVariables
    )
    {
        var textWithReplacedResourceVariables = TextWithResourceVariablesReplacer.ReplaceResourceVariables(
            textWithResourceVariables,
            CultureInfo.GetCultureInfo(cultureName)
        );
        
        textWithReplacedResourceVariables.ShouldBe(expectedTextWithReplacedResourceVariables);
    }
}