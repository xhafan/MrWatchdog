﻿using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Jobs.Queries;

[TsInterface]
public record JobAffectedEntityDto(string EntityName, long EntityId, bool IsCreated);