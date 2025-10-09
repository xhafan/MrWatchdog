using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Account.CompleteLogin;

namespace MrWatchdog.Web.Tests.Features.Account.CompleteLogin;

public class CompleteLoginControllerBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IJobCompletionAwaiter? _jobCompletionAwaiter;
    private IUrlHelper? _urlHelper;

    public CompleteLoginControllerBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public CompleteLoginControllerBuilder WitWithJobCompletionAwaiter(IJobCompletionAwaiter jobCompletionAwaiter)
    {
        _jobCompletionAwaiter = jobCompletionAwaiter;
        return this;
    }

    public CompleteLoginControllerBuilder WithUrlHelper(IUrlHelper urlHelper)
    {
        _urlHelper = urlHelper;
        return this;
    }

    public CompleteLoginController Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        _jobCompletionAwaiter ??= A.Fake<IJobCompletionAwaiter>();

        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetUserByEmailQueryHandler(
            unitOfWork,
            new UserRepository(unitOfWork)
        ));

        queryHandlerFactory.RegisterQueryHandler(new GetLoginTokenByGuidQueryHandler(
            unitOfWork,
            new LoginTokenRepository(unitOfWork)
        ));

        var controller = new CompleteLoginController(
            _bus,
            _jobCompletionAwaiter,
            new QueryExecutor(queryHandlerFactory),
            OptionsTestRetriever.Retrieve<JwtOptions>()
        );

        controller.Url = _urlHelper ?? A.Fake<IUrlHelper>();
        
        return controller;
    }
}