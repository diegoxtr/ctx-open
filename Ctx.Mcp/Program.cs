using Ctx.Infrastructure;
using Ctx.Mcp.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

var options = CtxMcpOptions.Parse(args);
var runtime = Bootstrapper.Create();
var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.AddSingleton(options);
builder.Services.AddSingleton(runtime);
builder.Services.AddSingleton(runtime.ApplicationService);
builder.Services.AddSingleton<RepositoryGuard>();
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
