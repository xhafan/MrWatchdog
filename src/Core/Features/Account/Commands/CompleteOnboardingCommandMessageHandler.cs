using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Account.Commands;

public class CompleteOnboardingCommandMessageHandler(
    IUserRepository userRepository
) 
    : IHandleMessages<CompleteOnboardingCommand>
{
    public async Task Handle(CompleteOnboardingCommand command)
    {
        var user = await userRepository.LoadByIdAsync(command.ActingUserId);

        user.CompleteOnboarding(command.OnboardingIdentifier);
    }
}