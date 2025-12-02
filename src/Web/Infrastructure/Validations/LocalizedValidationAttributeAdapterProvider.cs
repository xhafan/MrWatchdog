using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;
using MrWatchdog.Core.Resources;

namespace MrWatchdog.Web.Infrastructure.Validations;

public class LocalizedValidationAttributeAdapterProvider(IStringLocalizerFactory localizerFactory) : IValidationAttributeAdapterProvider
{
    private readonly ValidationAttributeAdapterProvider _baseProvider = new();
    private readonly IStringLocalizer _localizerToCheckIfTheResourceExists = localizerFactory.Create(typeof(Resource));

    public IAttributeAdapter? GetAttributeAdapter(
        ValidationAttribute attribute,
        IStringLocalizer? stringLocalizer
    )
    {
        var attributeType = attribute.GetType().Name;
        var localizedErrorMessage = _localizerToCheckIfTheResourceExists[attributeType];
        if (!localizedErrorMessage.ResourceNotFound)
        {
            attribute.ErrorMessageResourceName = attributeType;
            attribute.ErrorMessageResourceType = typeof(Resource);
            attribute.ErrorMessage = null;
        }

        return _baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
    }
}
