using System.Net;
using backend;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Listen(IPAddress.Any, Convert.ToInt32(Environment.GetEnvironmentVariable("PORT") ?? "5000"));
    });
}

var app = builder.ConfigureServices().ConfigurePipeline();

app.Run();