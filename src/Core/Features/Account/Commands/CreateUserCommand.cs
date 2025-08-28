using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Account.Commands;

public record CreateUserCommand(string Email) : Command;