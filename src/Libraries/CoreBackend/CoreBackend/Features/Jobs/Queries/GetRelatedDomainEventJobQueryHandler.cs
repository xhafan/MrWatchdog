using CoreBackend.Features.Jobs.Domain;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace CoreBackend.Features.Jobs.Queries;

public class GetRelatedDomainEventJobQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseNhibernateQueryHandler<GetRelatedDomainEventJobQuery, JobDto>(unitOfWork)
{
    public override async Task<IEnumerable<JobDto>> ExecuteAsync(GetRelatedDomainEventJobQuery query)
    {
        Job relatedCommandJob = null!;
        
        var domainEventJob = await Session.QueryOver<Job>()
            .JoinAlias(x => x.RelatedCommandJob, () => relatedCommandJob)
            .Where(x => relatedCommandJob.Guid == query.CommandJobGuid
                        && x.Type == query.Type)
            .SingleOrDefaultAsync();
        return domainEventJob == null
            ? []
            : [domainEventJob.GetDto()];
    }
}