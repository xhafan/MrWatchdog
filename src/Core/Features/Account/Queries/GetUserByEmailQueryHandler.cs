using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Account.Queries;

public class GetUserByEmailQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IUserRepository userRepository
) : BaseNhibernateQueryHandler<GetUserByEmailQuery>(unitOfWork)
{
    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetUserByEmailQuery query)
    {
        var user = await userRepository.GetByEmailAsync(query.Email);
        return user == null
            ? []
            : [(TResult) (object) user.GetDto()];
    }
}