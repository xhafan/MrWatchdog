using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Account.Commands;

public record CompleteOnboardingCommand(string OnboardingIdentifier) : Command;