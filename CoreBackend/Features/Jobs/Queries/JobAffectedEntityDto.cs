using Reinforced.Typings.Attributes;

namespace CoreBackend.Features.Jobs.Queries;

[TsInterface]
public record JobAffectedEntityDto(string EntityName, long EntityId, bool IsCreated);