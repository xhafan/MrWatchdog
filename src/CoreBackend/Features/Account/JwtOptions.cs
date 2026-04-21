namespace MrWatchdog.Core.Features.Account;

public class JwtOptions
{
    public string Key { get; set; } = null!;
    public int ExpireMinutes { get; set; }
}