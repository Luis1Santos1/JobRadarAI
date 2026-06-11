using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace JobRadar.Infrastructure.Normalization;

public sealed class ContactNormalizationService
{
    public string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);

            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var result = builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToUpperInvariant();

        result = Regex.Replace(result, @"\s+", " ");

        return result.Trim();
    }

    public string? NormalizeEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant();
    }

    public string? NormalizeLinkedInUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var raw = value.Trim();

        if (!raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            raw = $"https://{raw}";
        }

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
        {
            return value.Trim()
                .TrimEnd('/')
                .ToLowerInvariant();
        }

        var host = uri.Host
            .Replace("www.", string.Empty, StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

        if (host.EndsWith("linkedin.com", StringComparison.OrdinalIgnoreCase))
        {
            host = "linkedin.com";
        }

        var path = Uri.UnescapeDataString(uri.AbsolutePath)
            .Trim('/')
            .ToLowerInvariant();

        path = Regex.Replace(path, @"/+", "/");

        if (string.IsNullOrWhiteSpace(path))
        {
            return host;
        }

        return $"{host}/{path}";
    }
}