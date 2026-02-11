using System.Globalization;
using MrWatchdog.Core.Infrastructure.Localization;

namespace MrWatchdog.Core.Tests.Infrastructure.Localization.LocalizedStringResolvers;

[TestFixture]
public class when_resolving_localized_text
{
    [TestCase("""
              {
                "en": "English text",
                "cs": "Czech text"
              }
              """, "cs", "Czech text", TestName = "1 valid localized JSON with existing culture")]
    [TestCase("""
              {
                "en": "English text",
                "cs": "Czech text"
              }
              """, "de", "English text", TestName = "2 valid localized JSON with English translation, without existing culture")]
    [TestCase("""
              {
                "de": "German text",
                "cs": "Czech text"
              }
              """, "sk", "German text", TestName = "3 valid localized JSON without English translation, without existing culture")]
    [TestCase("Plain text", "cs", "Plain text", TestName = "4 plain text")]
    [TestCase("", "cs", "", TestName = "5 empty text")]
    [TestCase(null, "cs", null, TestName = "6 null text")]
    [TestCase("""
              {
                "en": "English text",
                "cs": 100
              }
              """, "cs", "English text", TestName = "7 localized JSON with invalid culture entry")]
    [TestCase("""
              {
                "en": "English text",
                "en": "english text",
                "cs": "Czech text"
              }
              """, "en", "english text", TestName = "8 valid localized JSON with duplicate cultures")]
    [TestCase("""
              {
                "en": "English text
              }
              """, "cs", 
              """
              {
                "en": "English text
              }
              """, TestName = "9 invalid JSON")]
    public void localized_text_is_resolved_correctly(
        string? text, 
        string cultureName, 
        string expectedText
    )
    {
        var localizedText = LocalizedTextResolver.ResolveLocalizedText(
            text,
            CultureInfo.GetCultureInfo(cultureName)
        );
        
        localizedText.ShouldBe(expectedText);
    }
}