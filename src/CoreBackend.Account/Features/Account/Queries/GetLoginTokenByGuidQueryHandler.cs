using CoreBackend.Features.Account.Domain;
using CoreBackend.Infrastructure.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace CoreBackend.Features.Account.Queries;

public class GetLoginTokenByGuidQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    ILoginTokenRepository loginTokenRepository
) : BaseNhibernateQueryHandler<GetLoginTokenByGuidQuery, LoginTokenDto>(unitOfWork)
{
    public override async Task<LoginTokenDto> ExecuteSingleAsync(GetLoginTokenByGuidQuery query)
    {
        var loginToken = await loginTokenRepository.LoadByGuidAsync(query.LoginTokenGuid);
        return loginToken.GetDto();
    }
}