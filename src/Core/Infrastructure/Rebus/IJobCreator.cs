using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public interface IJobCreator
{
    Task<Job> CreateJob(
        BaseMessage baseMessage, 
        Guid jobGuid, 
        bool shouldMarkJobAsHandlingStarted
    );
}