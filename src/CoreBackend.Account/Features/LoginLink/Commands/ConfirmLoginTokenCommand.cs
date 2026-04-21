using CoreBackend.Messages;

namespace CoreBackend.Account.Features.LoginLink.Commands;

public record ConfirmLoginTokenCommand(Guid LoginTokenGuid) : Command;