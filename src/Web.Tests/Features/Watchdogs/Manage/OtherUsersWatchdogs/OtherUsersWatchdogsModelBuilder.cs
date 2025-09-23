using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Manage.OtherUsersWatchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Manage.OtherUsersWatchdogs;

public class OtherUsersWatchdogsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private User? _actingUser;

    public OtherUsersWatchdogsModelBuilder WithActingUser(User actingUser)
    {
        _actingUser = actingUser;
        return this;
    }

    public OtherUsersWatchdogsModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        queryHandlerFactory.RegisterQueryHandler(new GetOtherUsersWatchdogsQueryHandler(unitOfWork));
        
        _actingUser ??= new UserBuilder(unitOfWork).Build();

        var actingUserAccessor = A.Fake<IActingUserAccessor>();
        A.CallTo(() => actingUserAccessor.GetActingUserId()).Returns(_actingUser.Id);

        var model = new OtherUsersWatchdogsModel(
            new QueryExecutor(queryHandlerFactory),
            actingUserAccessor
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}