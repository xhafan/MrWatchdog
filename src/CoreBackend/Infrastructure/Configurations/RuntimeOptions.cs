namespace CoreBackend.Infrastructure.Configurations;

public class RuntimeOptions
{
    public string Url { get; set; } = null!;
    public string? AppGitShaVersion { get; set; }
}