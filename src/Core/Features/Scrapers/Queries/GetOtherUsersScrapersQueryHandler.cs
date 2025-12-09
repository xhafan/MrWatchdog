using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using NHibernate;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetOtherUsersScrapersQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetOtherUsersScrapersQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    protected override IQueryOver GetQueryOver<TResult>(GetOtherUsersScrapersQuery query)
    {
        User userAlias = null!;
        GetOtherUsersScrapersQueryResult result = null!;

        return _unitOfWork.Session!.QueryOver<Scraper>()
            .JoinAlias(x => x.User, () => userAlias)
            .Where(x => userAlias.Id != query.UserId
                        && !x.IsArchived)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.ScraperId)
                .Select(x => x.Name).WithAlias(() => result.ScraperName)
                .Select(x => x.PublicStatus).WithAlias(() => result.PublicStatus)
                .Select(() => userAlias.Id).WithAlias(() => result.UserId)
                .Select(() => userAlias.Email).WithAlias(() => result.UserEmail)
            )
            .OrderByAlias(() => result.ScraperName).Asc
            .TransformUsing(Transformers.AliasToBean<GetOtherUsersScrapersQueryResult>());
    }
}