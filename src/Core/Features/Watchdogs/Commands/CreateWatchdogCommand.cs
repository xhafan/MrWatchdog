﻿using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record CreateWatchdogCommand(string Name) : Command;