using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Account.ConfirmLogin;

namespace MrWatchdog.Web.Tests.Features.Account.ConfirmLogin;

public class ConfirmLoginModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IJobCompletionAwaiter? _jobCompletionAwaiter;
    private string? _token;

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
    
    public ConfirmLoginModelBuilder WithToken(string? token)
    {
        _token = token;
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
            Token = _token!
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}