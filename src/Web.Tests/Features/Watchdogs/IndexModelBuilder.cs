using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Searches;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Searches;

public class SearchesModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private User? _actingUser;

    public SearchesModelBuilder WithActingUser(User actingUser)
    {
        _actingUser = actingUser;
        return this;
    }
    
    public SearchesModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogSearchesQueryHandler(unitOfWork));

        _actingUser ??= new UserBuilder(unitOfWork).Build();

        var actingUserAccessor = A.Fake<IActingUserAccessor>();
        A.CallTo(() => actingUserAccessor.GetActingUserId()).Returns(_actingUser.Id);
        
        var model = new SearchesModel(
            new QueryExecutor(queryHandlerFactory), 
            actingUserAccessor
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}