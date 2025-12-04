namespace MrWatchdog.Core.Resources;

public static class SharedTranslationsGetter
{
    public static SharedTranslations GetSharedTranslations()
    {
        return new SharedTranslations
        {
            Ok = Resource.Ok,
            Cancel = Resource.Cancel,
            Back = Resource.Back,
            Next = Resource.Next,
            Finish = Resource.Finish
        };
    }
}