using CoreBackend.Resources;

namespace MrWatchdog.Core.Resources;

public class SharedTranslationsGetter : ISharedTranslationsGetter
{
    private readonly SharedTranslations _sharedTranslations = new()
    {
        Ok = Resource.Ok,
        Cancel = Resource.Cancel,
        Back = Resource.Back,
        Next = Resource.Next,
        Finish = Resource.Finish,
        Error = Resource.Error,
        Edit = Resource.Edit,
        Save = Resource.Save
    };

    public SharedTranslations GetSharedTranslations()
    {
        return _sharedTranslations;
    }
}