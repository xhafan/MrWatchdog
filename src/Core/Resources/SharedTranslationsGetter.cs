using CoreBackend.Resources;

namespace MrWatchdog.Core.Resources;

public class SharedTranslationsGetter : ISharedTranslationsGetter
{
    private readonly SharedTranslations _sharedTranslations = new()
    {
        Ok = Resource.Ok,
        Cancel = Resource.Cancel,
        Error = Resource.Error,
        Edit = Resource.Edit,
        Save = Resource.Save,

        TranslationByResource = new Dictionary<string, string>
        {
            [nameof(Resource.Back)] = Resource.Back,
            [nameof(Resource.Next)] = Resource.Next,
            [nameof(Resource.Finish)] = Resource.Finish
        }
    };

    public SharedTranslations GetSharedTranslations()
    {
        return _sharedTranslations;
    }
}