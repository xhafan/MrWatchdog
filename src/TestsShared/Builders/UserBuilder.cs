using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class UserBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    private string _email = $"user+{Guid.NewGuid()}@email.com";

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }
    
    public User Build()
    {
        
        var user = new User(_email);

        unitOfWork?.Save(user);
        
        return user;
    }
}