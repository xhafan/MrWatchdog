using System.Globalization;
using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogsQuery(long UserId, CultureInfo Culture) : IQuery;