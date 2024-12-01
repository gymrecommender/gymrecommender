using backend;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://0.0.0.0:5000");
}
var app = builder.ConfigureServices().ConfigurePipeline();

app.Run();