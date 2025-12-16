using System.Diagnostics;
using System.Text;

namespace MrWatchdog.Core.Features.Scrapers.Services;

public class CurlScraper : IWebScraper
{
    public int Priority => 20;

    public async Task<ScrapeResult> Scrape(string url, ICollection<(string Name, string Value)>? httpHeaders)
    {
        var args = _BuildCurlArguments(url, httpHeaders);

        var psi = new ProcessStartInfo
        {
            FileName = "curl",
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,

            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = Process.Start(psi)!;

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var splitIndex = stdout.LastIndexOf("\nStatus: ", StringComparison.Ordinal);
        if (splitIndex < 0)
        {
            return ScrapeResult.Failed($"Unable to parse curl output. stderr={stderr}");
        }

        var html = stdout[..splitIndex];
        var statusPart = stdout[(splitIndex + 9)..].Trim();

        if (!int.TryParse(statusPart, out var statusCode))
        {
            return ScrapeResult.Failed($"Invalid status code from curl: {statusPart}");
        }

        return statusCode >= 200 && statusCode < 300
            ? ScrapeResult.Succeeded(html)
            : ScrapeResult.Failed($"Status code {statusCode}; {(stderr.Length > 0 ? stderr.Trim() : html)}");
    }

    private string _BuildCurlArguments(
        string url,
        ICollection<(string Name, string Value)>? headers
    )
    {
        var sb = new StringBuilder();
        sb.Append("-sS "); // silent mode but show error
        sb.Append("-L "); // follow redirects
        sb.Append("--http1.1 ");
        sb.Append("--tls-max 1.2 ");
        sb.Append("--compressed ");
        sb.Append("""
                  -w "\nStatus: %{http_code}" 
                  """
        );
       
        if (headers != null)
        {
            foreach (var (name, value) in headers)
            {
                sb.Append("-H ");
                sb.Append(
                    $"""
                     "{name}: {value}"
                     """
                );
                sb.Append(' ');
            }
        }

        sb.Append(
            $"""
             "{url}"
             """
        );

        return sb.ToString();
    }
}