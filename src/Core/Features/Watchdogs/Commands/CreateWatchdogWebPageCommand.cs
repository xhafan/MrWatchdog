﻿using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record CreateWatchdogWebPageCommand(long WatchdogId) : Command;