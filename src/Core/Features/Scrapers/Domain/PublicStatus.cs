namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public enum PublicStatus
{
    Private = 'T',
    RequestedToBeMadePublic = 'R',
    Public = 'P'
}