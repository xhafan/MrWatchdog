using CoreBackend.Messages;

namespace CoreBackend.Account.Features.LoginLink.Commands;

public record MarkLoginTokenAsUsedCommand(Guid LoginTokenGuid) : Command;