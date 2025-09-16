using System.Text;

namespace MrWatchdog.Core.Infrastructure.Extensions;

public static class HttpContentExtensions
{
    public static async Task<string> ReadAsStringWithLimitAsync(
        this HttpContent content, 
        int maxMegaBytes,
        string largeResponseErrorMessage
    )
    {
        var maxBytes = maxMegaBytes * 1024 * 1024;
        var contentLength = content.Headers.ContentLength;
        if (contentLength > maxBytes)
        {
            throw new InvalidOperationException(largeResponseErrorMessage);
        }

        var encoding = GetEncodingFromHeaders(content);

        await using var stream = await content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true);

        var buffer = new char[8192];
        var sb = new StringBuilder();
        long totalBytes = 0;

        while (true)
        {
            var read = await reader.ReadAsync(buffer, 0, buffer.Length);
            if (read == 0)
                break;

            totalBytes += encoding.GetByteCount(buffer.AsSpan(0, read));
            if (totalBytes > maxBytes)
            {
                throw new InvalidOperationException(largeResponseErrorMessage);
            }

            sb.Append(buffer, 0, read);
        }

        return sb.ToString();
    }

    private static Encoding GetEncodingFromHeaders(HttpContent content)
    {
        var contentTypeCharSet = content.Headers.ContentType?.CharSet;
        if (!string.IsNullOrWhiteSpace(contentTypeCharSet))
        {
            try
            {
                return Encoding.GetEncoding(contentTypeCharSet.Trim('"'));
            }
            catch (ArgumentException)
            {
                // Unknown encoding
            }
        }

        return Encoding.UTF8;
    }
}