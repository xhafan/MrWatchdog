using CoreBackend.Features.Jobs.Domain;
using CoreBackend.Messages;

namespace CoreBackend.Infrastructure.Rebus;

public interface IJobCreator
{
    Task<Job> CreateJob(
        BaseMessage baseMessage, 
        Guid jobGuid, 
        bool shouldMarkJobAsHandlingStarted,
        string? handlingQueue = null
    );
}