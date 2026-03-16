using System.Globalization;
using CoreBackend.Messages;

namespace MrWatchdog.Core.Features.Account.Commands;

public record CreateUserCommand(string Email, CultureInfo Culture) : Command;