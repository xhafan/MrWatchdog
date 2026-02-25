using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Account.Queries;

public class GetUserByEmailQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IUserRepository userRepository
) : BaseNhibernateQueryHandler<GetUserByEmailQuery, UserDto>(unitOfWork)
{
    public override async Task<IEnumerable<UserDto>> ExecuteAsync(GetUserByEmailQuery query)
    {
        var user = await userRepository.GetByEmailAsync(query.Email);
        return user == null
            ? []
            : [user.GetDto()];
    }
}