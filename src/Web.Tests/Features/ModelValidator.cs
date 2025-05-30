using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MrWatchdog.Web.Tests.Features;

public static class ModelValidator
{
    public static void ValidateModel(PageModel model)
    {
        var results = new List<ValidationResult>();
        
        var validator = new DataAnnotationsValidator.DataAnnotationsValidator();
        validator.TryValidateObjectRecursive(model, results);

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