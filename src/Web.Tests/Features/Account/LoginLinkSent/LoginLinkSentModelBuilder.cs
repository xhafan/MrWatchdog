using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Account.LoginLinkSent;

namespace MrWatchdog.Web.Tests.Features.Account.LoginLinkSent;

public class LoginLinkSentModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private Guid _loginTokenGuid;

    public LoginLinkSentModelBuilder WithLoginTokenGuid(Guid loginTokenGuid)
    {
        _loginTokenGuid = loginTokenGuid;
        return this;
    }
    
    public LoginLinkSentModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetLoginTokenByGuidQueryHandler(
            unitOfWork,
            new LoginTokenRepository(unitOfWork)
        ));

        var iJwtOptions = OptionsTestRetriever.Retrieve<JwtOptions>();

        var model = new LoginLinkSentModel(
            new QueryExecutor(queryHandlerFactory),
            iJwtOptions
        )
        {
            LoginTokenGuid = _loginTokenGuid
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}