using CoreBackend.Account.Features.Account;
using CoreBackend.Account.Features.Account.Queries;
using CoreBackend.Account.Infrastructure.Repositories;
using CoreBackend.Features.Jobs.Services;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.TestsShared;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Web.Features.Account.ConfirmLogin;

namespace MrWatchdog.Web.Tests.Features.Account.ConfirmLogin;

public class ConfirmLoginModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IJobCompletionAwaiter? _jobCompletionAwaiter;
    private string? _loginToken;

    public ConfirmLoginModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public ConfirmLoginModelBuilder WithJobCompletionAwaiter(IJobCompletionAwaiter jobCompletionAwaiter)
    {
        _jobCompletionAwaiter = jobCompletionAwaiter;
        return this;
    }
    
    public ConfirmLoginModelBuilder WithLoginToken(string? loginToken)
    {
        _loginToken = loginToken;
        return this;
    }
    
    public ConfirmLoginModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        _jobCompletionAwaiter ??= A.Fake<IJobCompletionAwaiter>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetLoginTokenByGuidQueryHandler(
            unitOfWork,
            new LoginTokenRepository(unitOfWork)
        ));        
        
        var model = new ConfirmLoginModel(
            _bus,
            OptionsTestRetriever.Retrieve<JwtOptions>(),
            new QueryExecutor(queryHandlerFactory),
            _jobCompletionAwaiter
        )
        {
            LoginToken = _loginToken!
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}