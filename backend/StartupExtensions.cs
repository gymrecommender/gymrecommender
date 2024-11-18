using backend.Enums;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using Npgsql;

namespace backend;

public static class StartupExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        Env.Load();
        string connectionString = $"Server={Environment.GetEnvironmentVariable("DB_HOST")};" +
                                  $"Database={Environment.GetEnvironmentVariable("DB_DATABASE")};" +
                                  $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                                  $"User Id={Environment.GetEnvironmentVariable("DB_USERNAME")};" +
                                  $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<AccountType>();
        dataSourceBuilder.MapEnum<OwnershipDecision>();
        dataSourceBuilder.MapEnum<ReccomendationType>();
        dataSourceBuilder.MapEnum<NotificationType>();
        dataSourceBuilder.MapEnum<ProviderType>();
        var dataSource = dataSourceBuilder.Build();
        
        builder.Services.AddSingleton<NpgsqlDataSource>(dataSource);
        builder.Services.AddDbContext<GymrecommenderContext>(options =>
            options.UseNpgsql(dataSource));
        
        builder.Services.AddCors();
        builder.Services.AddControllers();
        builder.Services.AddDbContext<GymrecommenderContext>(options =>
            options.UseNpgsql(connectionString));
        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseCors(aux =>
        {
            aux
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins("http://localhost:5173");
        });
        
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller}/{action=Index}/{id?}");

        app.MapFallbackToFile("/index.html");
        return app;
    }
}