using CoreBackend.Account.Features.Account.Queries;
using CoreBackend.Account.Infrastructure.Repositories;
using CoreBackend.TestsShared;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using CoreWeb.Account.Features.Account;

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