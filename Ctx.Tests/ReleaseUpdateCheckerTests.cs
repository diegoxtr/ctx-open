namespace Ctx.Tests;

using System.Net;
using Ctx.Cli;

public sealed class ReleaseUpdateCheckerTests
{
    [Fact]
    public async Task CheckLatestAsync_ReturnsUpdateAvailable_WhenLatestReleaseIsNewer()
    {
        using var httpClient = new HttpClient(new StubReleaseHandler(
            HttpStatusCode.OK,
            """{"tag_name":"v1.0.13","html_url":"https://github.com/diegoxtr/ctx-open/releases/tag/v1.0.13"}"""));
        var checker = new ReleaseUpdateChecker(httpClient);

        var status = await checker.CheckLatestAsync("1.0.11", null, null, CancellationToken.None);

        Assert.Equal("update-available", status.Status);
        Assert.True(status.UpdateAvailable);
        Assert.Equal("1.0.13", status.LatestVersion);
        Assert.Equal("v1.0.13", status.LatestTag);
        Assert.Equal("v1.0.11", status.CurrentTag);
        Assert.Null(status.Error);
    }

    [Fact]
    public async Task CheckLatestAsync_ReturnsCurrent_WhenLocalVersionMatchesLatestRelease()
    {
        using var httpClient = new HttpClient(new StubReleaseHandler(
            HttpStatusCode.OK,
            """{"tag_name":"v1.0.13","html_url":"https://github.com/diegoxtr/ctx-open/releases/tag/v1.0.13"}"""));
        var checker = new ReleaseUpdateChecker(httpClient);

        var status = await checker.CheckLatestAsync("1.0.13", "diegoxtr", "ctx-open", CancellationToken.None);

        Assert.Equal("current", status.Status);
        Assert.False(status.UpdateAvailable);
        Assert.Equal("1.0.13", status.LatestVersion);
        Assert.Null(status.Error);
    }

    [Fact]
    public async Task CheckLatestAsync_ReturnsUnavailable_WhenGitHubRequestFails()
    {
        using var httpClient = new HttpClient(new StubReleaseHandler(HttpStatusCode.Forbidden, "{}"));
        var checker = new ReleaseUpdateChecker(httpClient);

        var status = await checker.CheckLatestAsync("1.0.13", null, null, CancellationToken.None);

        Assert.Equal("unavailable", status.Status);
        Assert.False(status.UpdateAvailable);
        Assert.Null(status.LatestVersion);
        Assert.Contains("403", status.Error);
    }

    [Theory]
    [InlineData("v1.0.13", "1.0.13")]
    [InlineData("1.2.3-beta.1", "1.2.3")]
    [InlineData("release", null)]
    public void NormalizeReleaseVersion_ParsesExpectedPrefix(string input, string? expected)
    {
        Assert.Equal(expected, ReleaseUpdateChecker.NormalizeReleaseVersion(input));
    }

    private sealed class StubReleaseHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public StubReleaseHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("https://api.github.com/repos/diegoxtr/ctx-open/releases/latest", request.RequestUri?.ToString());

            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            });
        }
    }
}
