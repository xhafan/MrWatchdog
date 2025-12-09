using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using NHibernate;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetWatchdogSearchesQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetWatchdogSearchesQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    protected override IQueryOver GetQueryOver<TResult>(GetWatchdogSearchesQuery query)
    {
        Scraper scraper = null!;
        User user = null!;
        GetWatchdogSearchesQueryResult result = null!;

        return _unitOfWork.Session!.QueryOver<WatchdogSearch>()
            .JoinAlias(x => x.Scraper, () => scraper)
            .JoinAlias(x => x.User, () => user)
            .Where(x => user.Id == query.UserId
                        && !x.IsArchived
                        && !scraper.IsArchived)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogSearchId)
                .Select(() => scraper.Name).WithAlias(() => result.ScraperName)
                .Select(x => x.SearchTerm).WithAlias(() => result.SearchTerm)
                .Select(x => x.ReceiveNotification).WithAlias(() => result.ReceiveNotification)
            )
            .OrderBy(() => scraper.Name).Asc
            .ThenBy(x => x.SearchTerm).Asc
            .TransformUsing(Transformers.AliasToBean<GetWatchdogSearchesQueryResult>());
    }
}