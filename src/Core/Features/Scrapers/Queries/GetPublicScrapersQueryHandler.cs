using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Localization;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetPublicScrapersQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseNhibernateQueryHandler<GetPublicScrapersQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetPublicScrapersQuery query)
    {
        User userAlias = null!;
        GetPublicScrapersQueryResult result = null!;

        var results = await _unitOfWork.Session!.QueryOver<Scraper>()
            .JoinAlias(x => x.User, () => userAlias)
            .Where(x => x.PublicStatus == PublicStatus.Public
                        && !x.IsArchived)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.ScraperId)
                .Select(x => x.Name).WithAlias(() => result.ScraperName)
                .Select(() => userAlias.Id).WithAlias(() => result.UserId)
            )
            .OrderByAlias(() => result.ScraperName).Asc
            .TransformUsing(Transformers.AliasToBean<GetPublicScrapersQueryResult>())
            .ListAsync<GetPublicScrapersQueryResult>();;
        
        var localizedResults = results.Select(x => x with
            {
                ScraperName = LocalizedTextResolver.ResolveLocalizedText(x.ScraperName, query.Culture)
            })
            .ToList();
        
        return (IEnumerable<TResult>) localizedResults;
    }
}