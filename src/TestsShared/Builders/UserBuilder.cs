using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class UserBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    private string _email = $"user+{Guid.NewGuid()}@email.com";
    private bool _superAdmin;

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

    public User Build()
    {
        
        var user = new User(_email);
        if (_superAdmin)
        {
            user.MakeSuperAdmin();
        }

        unitOfWork?.Save(user);
        
        return user;
    }
}