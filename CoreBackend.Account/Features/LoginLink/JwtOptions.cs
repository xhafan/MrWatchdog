namespace CoreBackend.Account.Features.LoginLink;

public class JwtOptions
{
    public string Key { get; set; } = null!;
    public int ExpireMinutes { get; set; }
}