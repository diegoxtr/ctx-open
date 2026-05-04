namespace Ctx.Agent.Acp;

public sealed record AcpOptions(string RepositoryRoot, string Mode)
{
    public static AcpOptions Parse(IReadOnlyList<string> args)
    {
        var repositoryRoot = GetOption(args, "--repo") ?? Directory.GetCurrentDirectory();
        var mode = GetOption(args, "--mode") ?? "read-only";

        if (!string.Equals(mode, "read-only", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Ctx.Agent.Acp Phase 1 only supports --mode read-only.");
        }

        return new AcpOptions(Path.GetFullPath(repositoryRoot), mode);
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
