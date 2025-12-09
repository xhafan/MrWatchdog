using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Manage.OtherUsersScrapers;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Manage.OtherUsersScrapers;

public class OtherUsersScrapersModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private User? _actingUser;

    public OtherUsersScrapersModelBuilder WithActingUser(User actingUser)
    {
        _actingUser = actingUser;
        return this;
    }

    public OtherUsersScrapersModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        queryHandlerFactory.RegisterQueryHandler(new GetOtherUsersScrapersQueryHandler(unitOfWork));
        
        _actingUser ??= new UserBuilder(unitOfWork).Build();

        var actingUserAccessor = A.Fake<IActingUserAccessor>();
        A.CallTo(() => actingUserAccessor.GetActingUserId()).Returns(_actingUser.Id);

        var model = new OtherUsersScrapersModel(
            new QueryExecutor(queryHandlerFactory),
            actingUserAccessor
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}