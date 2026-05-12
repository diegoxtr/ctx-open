namespace Ctx.Cli;

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

public sealed class ReleaseUpdateChecker
{
    private const string DefaultOwner = "diegoxtr";
    private const string DefaultRepository = "ctx-open";

    private readonly HttpClient _httpClient;

    public ReleaseUpdateChecker(HttpClient httpClient)
    {
        _httpClient = httpClient;
        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ctx-cli", "1.0"));
        }
    }

    public async Task<ReleaseUpdateStatus> CheckLatestAsync(
        string currentVersion,
        string? owner,
        string? repository,
        CancellationToken cancellationToken)
    {
        var releaseOwner = NormalizeSourcePart(owner, "CTX_RELEASE_OWNER", DefaultOwner);
        var releaseRepository = NormalizeSourcePart(repository, "CTX_RELEASE_REPOSITORY", DefaultRepository);
        var releasesUrl = $"https://github.com/{releaseOwner}/{releaseRepository}/releases";
        var latestReleaseApiUrl = $"https://api.github.com/repos/{releaseOwner}/{releaseRepository}/releases/latest";

        try
        {
            using var response = await _httpClient.GetAsync(latestReleaseApiUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return ReleaseUpdateStatus.Unavailable(
                    currentVersion,
                    releasesUrl,
                    $"GitHub returned {(int)response.StatusCode} {response.StatusCode}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = document.RootElement;
            var latestTag = root.TryGetProperty("tag_name", out var tagElement) ? tagElement.GetString() : null;
            var latestReleaseUrl = root.TryGetProperty("html_url", out var urlElement) ? urlElement.GetString() : null;
            var latestVersion = NormalizeReleaseVersion(latestTag);
            var updateAvailable = CompareReleaseVersions(latestVersion, currentVersion) > 0;

            return new ReleaseUpdateStatus(
                updateAvailable ? "update-available" : "current",
                currentVersion,
                $"v{currentVersion}",
                latestVersion,
                latestTag,
                latestReleaseUrl ?? releasesUrl,
                updateAvailable,
                null,
                DateTimeOffset.UtcNow);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            return ReleaseUpdateStatus.Unavailable(currentVersion, releasesUrl, exception.Message);
        }
    }

    public static string? NormalizeReleaseVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = Regex.Match(value.Trim(), @"^v?(?<version>\d+(?:\.\d+){0,3})", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["version"].Value : null;
    }

    public static int CompareReleaseVersions(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return 0;
        }

        var leftParts = left.Split('.').Select(ParseVersionPart).ToArray();
        var rightParts = right.Split('.').Select(ParseVersionPart).ToArray();
        var length = Math.Max(leftParts.Length, rightParts.Length);
        for (var index = 0; index < length; index++)
        {
            var leftPart = index < leftParts.Length ? leftParts[index] : 0;
            var rightPart = index < rightParts.Length ? rightParts[index] : 0;
            var comparison = leftPart.CompareTo(rightPart);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return 0;
    }

    private static string NormalizeSourcePart(string? explicitValue, string environmentVariable, string fallback)
    {
        var value = !string.IsNullOrWhiteSpace(explicitValue)
            ? explicitValue
            : Environment.GetEnvironmentVariable(environmentVariable);

        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static int ParseVersionPart(string value)
        => int.TryParse(value, out var parsed) ? parsed : 0;
}

public sealed record ReleaseUpdateStatus(
    string Status,
    string CurrentVersion,
    string CurrentTag,
    string? LatestVersion,
    string? LatestTag,
    string LatestReleaseUrl,
    bool UpdateAvailable,
    string? Error,
    DateTimeOffset CheckedAtUtc)
{
    public static ReleaseUpdateStatus Unavailable(string currentVersion, string latestReleaseUrl, string error)
        => new(
            "unavailable",
            currentVersion,
            $"v{currentVersion}",
            null,
            null,
            latestReleaseUrl,
            false,
            error,
            DateTimeOffset.UtcNow);
}
