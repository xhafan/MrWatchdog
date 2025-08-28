using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Account.Queries;

public class GetLoginTokenConfirmationQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    ILoginTokenRepository loginTokenRepository
) : BaseNhibernateQueryHandler<GetLoginTokenConfirmationQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetLoginTokenConfirmationQuery query)
    {
        var loginToken = await loginTokenRepository.LoadByGuidAsync(query.LoginTokenGuid);
        return (TResult) (object) loginToken.Confirmed;
    }
}