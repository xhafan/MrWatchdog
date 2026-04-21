using CoreBackend.Infrastructure.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace CoreBackend.Features.Account.Queries;

public class GetLoginTokenConfirmationQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    ILoginTokenRepository loginTokenRepository
) : BaseNhibernateQueryHandler<GetLoginTokenConfirmationQuery, bool>(unitOfWork)
{
    public override async Task<bool> ExecuteSingleAsync(GetLoginTokenConfirmationQuery query)
    {
        var loginToken = await loginTokenRepository.LoadByGuidAsync(query.LoginTokenGuid);
        return loginToken.Confirmed;
    }
}