using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MrWatchdog.Web.Tests.Features;

public static class ModelValidator
{
    public static void ValidateModel(PageModel model)
    {
        var context = new ValidationContext(model, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        foreach (var validationResult in results)
        {
            var memberName = validationResult.MemberNames.FirstOrDefault() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(validationResult.ErrorMessage))
            {
                model.ModelState.AddModelError(memberName, validationResult.ErrorMessage);
            }
        }
    }
}