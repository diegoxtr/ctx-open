using Ctx.Agent.Acp;
using Ctx.Infrastructure;

var options = AcpOptions.Parse(args);
if (!Directory.Exists(Path.Combine(options.RepositoryRoot, ".ctx")))
{
    throw new InvalidOperationException($"No .ctx repository found at: {options.RepositoryRoot}");
}

var runtime = Bootstrapper.Create();
var handler = new AcpJsonRpcHandler(runtime.AgentService, runtime.JsonOptions, options.RepositoryRoot);

while (await Console.In.ReadLineAsync() is { } line)
{
    if (string.IsNullOrWhiteSpace(line))
    {
        continue;
    }

    var responses = await handler.HandleLineAsync(line, CancellationToken.None);
    foreach (var response in responses)
    {
        await Console.Out.WriteLineAsync(response);
        await Console.Out.FlushAsync();
    }
}
