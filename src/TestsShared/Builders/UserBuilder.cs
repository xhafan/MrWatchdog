using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Localization;
using System.Globalization;
using CoreDdd.Nhibernate.TestHelpers;

namespace MrWatchdog.TestsShared.Builders;

public class UserBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public static readonly CultureInfo Culture = CultureConstants.En;

    private string _email = $"user+{Guid.NewGuid()}@email.com";
    private bool _superAdmin;
    private CultureInfo _culture = Culture;

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }
    
    public UserBuilder WithSuperAdmin(bool superAdmin)
    {
        _superAdmin = superAdmin;
        return this;
    }

    public UserBuilder WithCulture(CultureInfo culture)
    {
        _culture = culture;
        return this;
    }

    public User Build()
    {
        var user = new User(_email, _culture);
        if (_superAdmin)
        {
            user.MakeSuperAdmin();
        }

        unitOfWork?.Save(user);
        
        return user;
    }
}