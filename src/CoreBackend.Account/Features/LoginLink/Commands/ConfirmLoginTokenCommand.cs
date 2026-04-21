using CoreBackend.Messages;

namespace CoreBackend.Account.Features.Account.Commands;

public record ConfirmLoginTokenCommand(Guid LoginTokenGuid) : Command;