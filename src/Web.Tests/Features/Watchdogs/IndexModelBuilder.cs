using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs;

public class IndexModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private User? _actingUser;

    public IndexModelBuilder WithActingUser(User actingUser)
    {
        _actingUser = actingUser;
        return this;
    }
    
    public IndexModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogsQueryHandler(unitOfWork));

        _actingUser ??= new UserBuilder(unitOfWork).Build();

        var actingUserAccessor = A.Fake<IActingUserAccessor>();
        A.CallTo(() => actingUserAccessor.GetActingUserId()).Returns(_actingUser.Id);
        
        var model = new IndexModel(
            new QueryExecutor(queryHandlerFactory), 
            actingUserAccessor
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}