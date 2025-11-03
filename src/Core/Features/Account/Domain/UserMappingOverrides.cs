using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace MrWatchdog.Core.Features.Account.Domain;

public class UserMappingOverrides : IAutoMappingOverride<User>
{
    public void Override(AutoMapping<User> mapping)
    {
        mapping.HasMany(x => x.CompleteOnboardings)
            .Table("UserCompleteOnboarding")
            .Element("OnboardingIdentifier", x =>
            {
                x.Not.Nullable();
                x.Length(10000); // force it to map it to text DB type
            });
    }
}