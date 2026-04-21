using CoreBackend.Account.Features.Account.Domain;
using CoreBackend.Account.Infrastructure.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace CoreBackend.Account.Features.Account.Queries;

public class GetLoginTokenByIdQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    ILoginTokenRepository loginTokenRepository
) : BaseNhibernateQueryHandler<GetLoginTokenByIdQuery, LoginTokenDto>(unitOfWork)
{
    public override async Task<LoginTokenDto> ExecuteSingleAsync(GetLoginTokenByIdQuery query)
    {
        var loginToken = await loginTokenRepository.LoadByIdAsync(query.LoginTokenId);
        return loginToken.GetDto();
    }
}