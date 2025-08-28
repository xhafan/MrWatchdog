using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Account.Login;

namespace MrWatchdog.Web.Tests.Features.Account.Login;

public class LoginModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    public const string Email = "user@email.com";
    public const string ReturnUrl = "/Watchdogs/Alerts";
    
    private string _email = Email;
    private string? _returnUrl = ReturnUrl;

    private ICoreBus? _bus;
    private IJobCompletionAwaiter? _jobCompletionAwaiter;

    public LoginModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public LoginModelBuilder WithJobCompletionAwaiter(IJobCompletionAwaiter jobCompletionAwaiter)
    {
        _jobCompletionAwaiter = jobCompletionAwaiter;
        return this;
    }

    public LoginModelBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public LoginModelBuilder WithReturnUrl(string? returnUrl)
    {
        _returnUrl = returnUrl;
        return this;
    }

    public LoginModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        _jobCompletionAwaiter ??= A.Fake<IJobCompletionAwaiter>();

        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetJobQueryHandler(
            unitOfWork,
            new JobRepository(unitOfWork)
        ));

        queryHandlerFactory.RegisterQueryHandler(new GetLoginTokenByIdQueryHandler(
            unitOfWork,
            new LoginTokenRepository(unitOfWork)
        ));

        var model = new LoginModel(
            _bus,
            _jobCompletionAwaiter,
            new QueryExecutor(queryHandlerFactory)
        )
        {
            Email = _email,
            ReturnUrl = _returnUrl,
            Url = new UrlHelper(new ActionContext {RouteData = new RouteData()})
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}