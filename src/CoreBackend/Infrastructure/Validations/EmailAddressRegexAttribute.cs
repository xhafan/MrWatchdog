using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Core.Infrastructure.Validations;

public class EmailAddressRegexAttribute : RegularExpressionAttribute
{
    private const string EmailRegexPattern = @"^[^\s@]+@([^\s@.,]+\.)+[^\s@.,]{2,}$"; // regex pattern taken from https://stackoverflow.com/a/201447/379279 's comment
    private const string EmailRegexPatternAcceptingSpacesAround = @"^[^@]+@([^\s@.,]+\.)+[^@.,]{2,}$";

    public EmailAddressRegexAttribute(bool acceptSpacesAroundEmail = false) 
        : base(acceptSpacesAroundEmail 
            ? EmailRegexPatternAcceptingSpacesAround 
            : EmailRegexPattern)
    {
        ErrorMessage = "The {0} field is not a valid email address";
    }
}