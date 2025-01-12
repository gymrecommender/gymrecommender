using System.Reflection;
using backend.Enums;
using backend.Models;
using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;

namespace backend;

public static class StartupExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        Env.Load();
        string connectionString = $"Server=localhost;" +
                                  $"Database=gym;" +
                                  $"Port={5432};" +
                                  $"User Id=postgres;" +
                                  $"Password=postgres;";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        dataSourceBuilder.MapEnum<AccountType>("account_type");
        dataSourceBuilder.MapEnum<OwnershipDecision>("own_decision");
        dataSourceBuilder.MapEnum<RecommendationType>("rec_type");
        dataSourceBuilder.MapEnum<NotificationType>("not_type");
        dataSourceBuilder.MapEnum<ProviderType>("provider_type");

        var dataSource = dataSourceBuilder.Build();

        builder.Services.AddSingleton<NpgsqlDataSource>(dataSource);
        builder.Services.AddDbContext<GymrecommenderContext>(options =>
            options.UseNpgsql(dataSource));

        //builder.Services.AddCors();
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; });
        ;

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "GymRecommender Web API",
                Version = "v1"
            });
            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            c.IncludeXmlComments(xmlPath);
        });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto
        });
        string pathBase = app.Configuration["PathBase"];
        if (!string.IsNullOrWhiteSpace(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseHsts().UseHttpsRedirection();
        }
        else
        {
            app.UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json",
                        "GymRecommender WebAPI");
                    c.RoutePrefix = "docs";
                });
        }

        app
            // .UseCors(aux =>
            //     {
            //         aux
            //             .AllowAnyHeader()
            //             .AllowAnyMethod()
            //             .WithOrigins(Environment.GetEnvironmentVariable("FRONTEND_ADDRESS"))
            //             .AllowCredentials();
            //     })
            .UseStaticFiles()
            .UseRouting()
            .UseEndpoints(endpoints => { endpoints.MapControllers(); });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller}/{action=Index}/{id?}");

        return app;
    }
}