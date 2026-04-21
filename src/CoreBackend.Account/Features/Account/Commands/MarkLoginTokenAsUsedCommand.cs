using CoreBackend.Messages;

namespace CoreBackend.Account.Features.Account.Commands;

public record MarkLoginTokenAsUsedCommand(Guid LoginTokenGuid) : Command;