using CoreBackend.Messages;

namespace CoreBackend.Features.Account.Commands;

public record ConfirmLoginTokenCommand(Guid LoginTokenGuid) : Command;