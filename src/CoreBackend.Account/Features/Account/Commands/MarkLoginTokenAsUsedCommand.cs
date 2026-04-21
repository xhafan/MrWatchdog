using CoreBackend.Messages;

namespace CoreBackend.Features.Account.Commands;

public record MarkLoginTokenAsUsedCommand(Guid LoginTokenGuid) : Command;