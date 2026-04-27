namespace Ctx.Mcp.Mcp;

public sealed class RepositoryGuard
{
    private readonly CtxMcpOptions _options;

    public RepositoryGuard(CtxMcpOptions options)
    {
        _options = options;
    }

    public string Resolve(string? repositoryPath = null)
    {
        var candidate = string.IsNullOrWhiteSpace(repositoryPath)
            ? _options.RepositoryRoot
            : Path.GetFullPath(repositoryPath);

        if (!Directory.Exists(candidate))
        {
            throw new InvalidOperationException($"Repository path does not exist: {candidate}");
        }

        if (!Directory.Exists(Path.Combine(candidate, ".ctx")))
        {
            throw new InvalidOperationException($"No .ctx repository found at: {candidate}");
        }

        var isConfiguredRepository = string.Equals(candidate, _options.RepositoryRoot, StringComparison.OrdinalIgnoreCase);

        if (!isConfiguredRepository && _options.AllowedRoot is null)
        {
            throw new InvalidOperationException("Dynamic repo paths are disabled by default. Configure a separate MCP server entry or start ctx-mcp with --allow-root.");
        }

        if (_options.AllowedRoot is not null && !IsSameOrChild(candidate, _options.AllowedRoot))
        {
            throw new InvalidOperationException($"Repository path is outside the allowed root: {_options.AllowedRoot}");
        }

        return candidate;
    }

    public string ResolveWritable(string? repositoryPath = null)
    {
        if (_options.IsReadOnly)
        {
            throw new InvalidOperationException("This CTX MCP server is running in read-only mode. Restart ctx-mcp with --mode write to enable write tools.");
        }

        return Resolve(repositoryPath);
    }

    private static bool IsSameOrChild(string candidate, string allowedRoot)
    {
        var normalizedCandidate = Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidate));
        var normalizedAllowedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(allowedRoot));

        return normalizedCandidate.Equals(normalizedAllowedRoot, StringComparison.OrdinalIgnoreCase)
            || normalizedCandidate.StartsWith(normalizedAllowedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || normalizedCandidate.StartsWith(normalizedAllowedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
