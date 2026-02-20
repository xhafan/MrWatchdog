using MrWatchdog.Core.Infrastructure;
using System.Security.Cryptography;
using System.Text;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class ScrapedResult
{
    protected ScrapedResult() {}

    public ScrapedResult(string value)
    {
        Value = value;
        Hash = _CalculateHash(value);
    }

    public string Value { get; } = null!;
    public byte[] Hash { get; private set; } = null!;

    private byte[] _CalculateHash(string result)
    {
        var text = HtmlExtractor.ExtractTextFromHtml(result);
        var linkUrls = HtmlExtractor.ExtractLinkUrlsFromHtml(result);

        var hashingSource = new StringBuilder();

        hashingSource.Append("TEXT:\n");
        hashingSource.Append(text.ToLowerInvariant());
        hashingSource.Append("\n");
        hashingSource.Append("LINKS:\n");

        foreach (var link in linkUrls)
        {
            hashingSource.Append(link.ToLowerInvariant());
            hashingSource.Append("\n");
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(hashingSource.ToString()));
    }

    public void RecalculateHash()
    {
        Hash = _CalculateHash(Value);
    }

    public bool Equals(ScrapedResult? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Hash.SequenceEqual(other.Hash);
    }

    public override bool Equals(object? obj)
    {
        return obj is ScrapedResult other && Equals(other);
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return BitConverter.ToInt32(Hash, 0); // SHA256 output has first 4 bytes are well distributed
    }

    public static bool operator ==(ScrapedResult? left, ScrapedResult? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ScrapedResult? left, ScrapedResult? right)
    {
        return !(left == right);
    }

}