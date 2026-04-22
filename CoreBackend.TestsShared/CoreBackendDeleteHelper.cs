using CoreBackend.Features.Jobs.Domain;
using CoreBackend.Features.Jobs.Services;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Repositories;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace CoreBackend.TestsShared;

public static class CoreBackendDeleteHelper
{
    extension(NhibernateUnitOfWork unitOfWork)
    {
        public async Task DeleteJobCascade(Guid jobGuid, bool waitForJobCompletion = false)
        {
            var jobRepository = new JobRepository(unitOfWork);
            var job = await jobRepository.GetByGuidAsync(jobGuid);
            await unitOfWork.DeleteJobCascade(job, waitForJobCompletion);
        }

        public async Task DeleteJobCascade(Job? job, bool waitForJobCompletion = false)
        {
            if (job == null) return;

            var jobRepository = new JobRepository(unitOfWork);
            job = await jobRepository.GetAsync(job.Id);
            if (job == null) return;

            if (waitForJobCompletion)
            {
                var rebusOptions = OptionsTestRetriever.Retrieve<RebusOptions>();
                var jobCompletionAwaiter = new JobCompletionAwaiter(TestFixtureContext.NhibernateConfigurator, rebusOptions);
                await jobCompletionAwaiter.WaitForJobCompletion(job.Guid);
        
                await unitOfWork.Session!.EvictAsync(job);
                job = await jobRepository.LoadByIdAsync(job.Id);
            }

            await jobRepository.DeleteAsync(job);
            await unitOfWork.FlushAsync();
        }
    }
}