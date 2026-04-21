using CoreBackend.Account.Features.LoginLink.Queries;
using CoreBackend.Account.Infrastructure.Repositories;
using CoreBackend.Features.Jobs.Queries;
using CoreBackend.Features.Jobs.Services;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Repositories;
using CoreBackend.TestsShared;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using MrWatchdog.Web.Features.Account.LoginLink.Login;

namespace MrWatchdog.Web.Tests.Features.Account.LoginLink.Login;

public class LoginModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    public const string Email = "user@email.com";
    public const string ReturnUrl = "/Watchdogs";
    
    private string _email = Email;
    private string? _returnUrl = ReturnUrl;

    private ICoreBus? _bus;
    private IJobCompletionAwaiter? _jobCompletionAwaiter;
    private IUrlHelper? _urlHelper;

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

    public LoginModelBuilder WithUrlHelper(IUrlHelper urlHelper)
    {
        _urlHelper = urlHelper;
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

        _urlHelper ??= A.Fake<IUrlHelper>();
        A.CallTo(() => _urlHelper.IsLocalUrl(ReturnUrl)).Returns(true);

        model.Url = _urlHelper ?? A.Fake<IUrlHelper>();

        return model;
    }
}