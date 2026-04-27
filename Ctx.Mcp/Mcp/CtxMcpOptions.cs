namespace Ctx.Mcp.Mcp;

public sealed record CtxMcpOptions(string RepositoryRoot, string Mode, string? AllowedRoot)
{
    public bool IsReadOnly => !string.Equals(Mode, "write", StringComparison.OrdinalIgnoreCase);

    public static CtxMcpOptions Parse(IReadOnlyList<string> args)
    {
        var repositoryRoot = GetOption(args, "--repo") ?? Directory.GetCurrentDirectory();
        var mode = GetOption(args, "--mode") ?? "read-only";
        var allowedRoot = GetOption(args, "--allow-root");

        if (!string.Equals(mode, "read-only", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(mode, "write", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("--mode must be read-only or write.");
        }

        return new CtxMcpOptions(
            Path.GetFullPath(repositoryRoot),
            mode,
            string.IsNullOrWhiteSpace(allowedRoot) ? null : Path.GetFullPath(allowedRoot));
    }

    private static string? GetOption(IReadOnlyList<string> args, string name)
    {
        for (var index = 0; index < args.Count; index++)
        {
            if (!string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return index + 1 < args.Count ? args[index + 1] : null;
        }

        return null;
    }
}
