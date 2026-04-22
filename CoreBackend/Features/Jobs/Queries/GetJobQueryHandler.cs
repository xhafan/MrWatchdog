using CoreBackend.Infrastructure.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace CoreBackend.Features.Jobs.Queries;

public class GetJobQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IJobRepository jobRepository
) : BaseNhibernateQueryHandler<GetJobQuery, JobDto>(unitOfWork)
{
    public override async Task<IEnumerable<JobDto>> ExecuteAsync(GetJobQuery query)
    {
        var job = await jobRepository.GetByGuidAsync(query.JobGuid);
        return job == null
            ? []
            : [job.GetDto()];
    }
}