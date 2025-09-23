using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Manage.UserWatchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Manage.UserWatchdogs;

public class UserWatchdogsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private User? _actingUser;

    public UserWatchdogsModelBuilder WithActingUser(User actingUser)
    {
        _actingUser = actingUser;
        return this;
    }

    public UserWatchdogsModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        queryHandlerFactory.RegisterQueryHandler(new GetUserWatchdogsQueryHandler(unitOfWork));
        
        _actingUser ??= new UserBuilder(unitOfWork).Build();

        var actingUserAccessor = A.Fake<IActingUserAccessor>();
        A.CallTo(() => actingUserAccessor.GetActingUserId()).Returns(_actingUser.Id);

        var model = new UserWatchdogsModel(
            new QueryExecutor(queryHandlerFactory),
            actingUserAccessor
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}