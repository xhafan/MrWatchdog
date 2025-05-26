using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Jobs.Queries;

namespace MrWatchdog.Web.Features.Jobs;

[ApiController]
[Route("api/[controller]")]
public class JobsController(IQueryExecutor queryExecutor) : ControllerBase
{
    [HttpGet("{jobGuid}")]
    public async Task<JobDto> GetJob(Guid jobGuid)
    {
        return (
            await queryExecutor.ExecuteAsync<GetJobQuery, JobDto>(new GetJobQuery(jobGuid))
        ).Single();
    }
}