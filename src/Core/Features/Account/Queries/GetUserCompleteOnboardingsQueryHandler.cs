using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using NHibernate;
using NHibernate.Criterion;

namespace MrWatchdog.Core.Features.Account.Queries;

public class GetUserCompleteOnboardingsQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetUserCompleteOnboardingsQuery>(unitOfWork)
{
    protected override IQueryOver GetQueryOver<TResult>(GetUserCompleteOnboardingsQuery query)
    {
        string completeOnboardingsAlias = null!;

        return Session.QueryOver<User>()
            .JoinAlias(x => x.CompleteOnboardings, () => completeOnboardingsAlias)
            .Where(x => x.Id == query.UserId)
            .Select(Projections.Property(nameof(completeOnboardingsAlias) + ".elements")); // More info about ".elements": https://github.com/nhibernate/nhibernate-core/issues/3235#issuecomment-1425811847
    }
}