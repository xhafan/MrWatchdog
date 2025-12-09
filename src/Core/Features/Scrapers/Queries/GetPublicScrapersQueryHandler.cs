using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using NHibernate;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetPublicScrapersQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetPublicScrapersQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    protected override IQueryOver GetQueryOver<TResult>(GetPublicScrapersQuery query)
    {
        User userAlias = null!;
        GetPublicScrapersQueryResult result = null!;

        return _unitOfWork.Session!.QueryOver<Scraper>()
            .JoinAlias(x => x.User, () => userAlias)
            .Where(x => x.PublicStatus == PublicStatus.Public
                        && !x.IsArchived)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.ScraperId)
                .Select(x => x.Name).WithAlias(() => result.ScraperName)
                .Select(() => userAlias.Id).WithAlias(() => result.UserId)
            )
            .OrderByAlias(() => result.ScraperName).Asc
            .TransformUsing(Transformers.AliasToBean<GetPublicScrapersQueryResult>());
    }
}