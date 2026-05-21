namespace Ctx.Tests;

public sealed class DistributionPackagingContractTests
{
    [Fact]
    public async Task DistributionBuild_UsesPackagedDocumentationManifest()
    {
        var buildScript = await ReadRepositoryFileAsync("scripts", "build-distribution.ps1");
        var installScript = await ReadRepositoryFileAsync("scripts", "install-ctx.ps1");
        var shellInstallScript = await ReadRepositoryFileAsync("scripts", "install-ctx.sh");

        Assert.Contains("distribution\\packaged-docs.txt", buildScript);
        Assert.Contains("distribution\\packaged-prompts.txt", buildScript);
        Assert.Contains("Copy-PackagedPromptAssets", buildScript);
        Assert.Contains("distribution\\packaged-docs.txt", installScript);
        Assert.Contains("distribution\\packaged-prompts.txt", installScript);
        Assert.Contains("validate_packaged_assets", shellInstallScript);
        Assert.Contains("packaged-docs.txt", shellInstallScript);
        Assert.Contains("packaged-prompts.txt", shellInstallScript);
    }

    [Fact]
    public async Task PackagedDocumentationManifest_IncludesReleaseCriticalDocs()
    {
        var packagedDocs = await ReadRepositoryFileAsync("distribution", "packaged-docs.txt");
        var packagedPrompts = await ReadRepositoryFileAsync("distribution", "packaged-prompts.txt");

        Assert.Contains("README.md", packagedDocs);
        Assert.Contains("CHANGELOG.md", packagedDocs);
        Assert.Contains("docs/TECHNICAL_INDEX.md", packagedDocs);
        Assert.Contains("docs/INSTALLATION_AND_USAGE_GUIDE.md", packagedDocs);
        Assert.Contains("docs/INSTALLER_AND_DISTRIBUTION.md", packagedDocs);
        Assert.Contains("docs/CLI_COMMANDS.md", packagedDocs);
        Assert.Contains("docs/CTX_VIEWER_GUIDE.md", packagedDocs);
        Assert.Contains("docs/CTX_MCP_AGENT_SETUP.md", packagedDocs);
        Assert.Contains("docs/OPERATIONAL_RUNBOOKS.md", packagedDocs);
        Assert.Contains("docs/RELEASE_1_0_22.md", packagedDocs);
        Assert.DoesNotContain("docs/private", packagedDocs, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("prompts/CTX_HELPER_PROMPT.md", packagedPrompts);
        Assert.Contains("prompts/CTX_AGENT_PROMPT.md", packagedPrompts);
        Assert.Contains("prompts/CTX_AUTONOMOUS_OPERATOR_PROMPT.md", packagedPrompts);
    }

    private static Task<string> ReadRepositoryFileAsync(params string[] relativeParts)
        => File.ReadAllTextAsync(FindRepositoryFile(relativeParts));

    private static string FindRepositoryFile(params string[] relativeParts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativeParts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file '{Path.Combine(relativeParts)}'.");
    }
}
