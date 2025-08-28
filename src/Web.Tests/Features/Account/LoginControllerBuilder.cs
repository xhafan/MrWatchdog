using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Account;

namespace MrWatchdog.Web.Tests.Features.Account;

public class LoginControllerBuilder(NhibernateUnitOfWork unitOfWork)
{
    public LoginController Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetLoginTokenConfirmationQueryHandler(
            unitOfWork,
            new LoginTokenRepository(unitOfWork)
        ));

        var controller = new LoginController(
            new QueryExecutor(queryHandlerFactory)
        );
        
        return controller;
    }
}