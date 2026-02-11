using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Localization;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetUserScrapersQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseNhibernateQueryHandler<GetUserScrapersQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetUserScrapersQuery query)
    {
        GetUserScrapersQueryResult result = null!;
        User user = null!;

        var results = await _unitOfWork.Session!.QueryOver<Scraper>()
            .JoinAlias(x => x.User, () => user)
            .Where(x => user.Id == query.UserId
                        && !x.IsArchived)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.ScraperId)
                .Select(x => x.Name).WithAlias(() => result.ScraperName)
                .Select(x => x.PublicStatus).WithAlias(() => result.PublicStatus)
            )
            .OrderByAlias(() => result.ScraperName).Asc
            .TransformUsing(Transformers.AliasToBean<GetUserScrapersQueryResult>())
            .ListAsync<GetUserScrapersQueryResult>();

        var localizedResults = results.Select(x => x with
            {
                ScraperName = LocalizedTextResolver.ResolveLocalizedText(x.ScraperName, query.Culture)
            })
            .ToList();
        
        return (IEnumerable<TResult>) localizedResults;
    }
}