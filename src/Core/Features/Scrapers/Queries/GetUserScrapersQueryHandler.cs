using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using NHibernate;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetUserScrapersQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetUserScrapersQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    protected override IQueryOver GetQueryOver<TResult>(GetUserScrapersQuery query)
    {
        GetUserScrapersQueryResult result = null!;
        User user = null!;

        return _unitOfWork.Session!.QueryOver<Scraper>()
            .JoinAlias(x => x.User, () => user)
            .Where(x => user.Id == query.UserId
                        && !x.IsArchived)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.ScraperId)
                .Select(x => x.Name).WithAlias(() => result.ScraperName)
                .Select(x => x.PublicStatus).WithAlias(() => result.PublicStatus)
            )
            .OrderByAlias(() => result.ScraperName).Asc
            .TransformUsing(Transformers.AliasToBean<GetUserScrapersQueryResult>());
    }
}