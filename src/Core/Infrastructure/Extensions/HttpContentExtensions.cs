using System.IO.Compression;
using System.Text;
using CoreUtils.Extensions;

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
            throw new HttpResponseTooLargeException(largeResponseErrorMessage);
        }

        var encoding = _GetEncodingFromHeaders(content);

        await using var contentStream = await content.ReadAsStreamAsync();
        await using var decompressedStream = _GetDecompressedStream(contentStream, content);
        using var reader = new StreamReader(decompressedStream, encoding, detectEncodingFromByteOrderMarks: true);

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
                throw new HttpResponseTooLargeException(largeResponseErrorMessage);
            }

            sb.Append(buffer, 0, read);
        }

        return sb.ToString();
    }

    private static Encoding _GetEncodingFromHeaders(HttpContent content)
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

    private static Stream _GetDecompressedStream(Stream contentStream, HttpContent content)
    {
        var contentEncoding = content.Headers.ContentEncoding;
        
        if (contentEncoding.IsEmpty())
        {
            return contentStream;
        }

        var encoding = contentEncoding.FirstOrDefault()?.ToLowerInvariant();
        
        return encoding switch
        {
            "gzip" => new GZipStream(contentStream, CompressionMode.Decompress),
            "deflate" => new DeflateStream(contentStream, CompressionMode.Decompress),
            "br" => new BrotliStream(contentStream, CompressionMode.Decompress),
            _ => contentStream
        };
    }
}