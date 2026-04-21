using CoreBackend.Account.Features.LoginLink;
using CoreBackend.Account.Features.LoginLink.Queries;
using CoreBackend.Account.Infrastructure.Repositories;
using CoreBackend.Features.Jobs.Services;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.TestsShared;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Web.Features.Account.LoginLink.CompleteLogin;

namespace MrWatchdog.Web.Tests.Features.Account.LoginLink.CompleteLogin;

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