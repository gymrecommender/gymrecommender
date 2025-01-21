using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using backend;
using backend.Authorization;
using backend.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

//================================================

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new backend.Utilities.TimeOnlyJsonConverter());
});

//================================================

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase,
        allowIntegerValues: false));
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
;

var fireBaseProjectId = Environment.GetEnvironmentVariable("JWT_AUTHORITY")
                        ?? throw new InvalidOperationException("JWT_AUTHORITY not set.");
;

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = "https://securetoken.google.com/" + fireBaseProjectId;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://securetoken.google.com/" + fireBaseProjectId,
            ValidateAudience = true,
            ValidAudience = fireBaseProjectId,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        // Hook into the JWT Bearer events for logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("Authentication failed.", context.Exception);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated for {User}", context.Principal.Identity.Name);
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token received from request.");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("OnChallenge error: {Error}, description: {ErrorDescription}", context.Error,
                    context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization()
    .AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.Requirements.Add(new HasTypeRequirement("admin")));
        options.AddPolicy("GymOnly", policy => policy.Requirements.Add(new HasTypeRequirement("gym")));
        options.AddPolicy("UserOnly", policy => policy.Requirements.Add(new HasTypeRequirement("user")));
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, AuthorizationRequestHandler>();
builder.Services.AddScoped<RecommendationService, RecommendationService>();
builder.Services.AddScoped<GeoService, GeoService>();
builder.Services.AddScoped<GymRetrievalService, GymRetrievalService>();
builder.Services.AddScoped<AuthenticationService, AuthenticationService>();


if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        var port = Convert.ToInt32(Environment.GetEnvironmentVariable("PORT") ?? "5000");
        options.Listen(IPAddress.Any, port);
    });
}

var app = builder.ConfigureServices().ConfigurePipeline();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();