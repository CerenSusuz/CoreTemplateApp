using System.Reflection;
using System.Text;
using Core.AI.Abstractions;
using Core.AI.Commands;
using Core.AI.Config;
using Core.AI.Providers;
using Core.AI.Providers.Ollama;
using Core.AI.Providers.OpenRouter;
using CoreApp.Application.Common.Behaviors;
using CoreApp.Application.Common.Interfaces.Auth;
using CoreApp.Application.Common.Settings;
using CoreApp.Infrastructure.Data;
using CoreApp.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- AI Services ---
builder.Services.AddOptions<AISettings>()
    .Bind(builder.Configuration.GetSection("AiSettings"))
    .Validate(settings => Enum.IsDefined(typeof(AIProvider), settings.Provider),
        "Invalid AI provider configured in AiSettings.Provider");

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<AISettings>>().Value);

// Providers
builder.Services.AddScoped<OpenRouterAiService>();
builder.Services.AddScoped<OllamaAiService>();
builder.Services.AddScoped<IAIService, AIServiceResolver>();

// Model Providers
builder.Services.AddScoped<OpenRouterModelProvider>();
builder.Services.AddScoped<OllamaModelProvider>();
builder.Services.AddScoped<AIModelProviderResolver>();

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreApp API", Version = "v1" });

    // 🔐 Swagger JWT support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }
});

});

builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// EF Core DbContext (InMemory - dev only)
//builder.Services.AddDbContext<CoreAppDbContext>(options =>
//    options.UseInMemoryDatabase("CoreAppDb"));

builder.Services.AddDbContext<CoreAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Bind JwtSettings from config
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration
    .GetSection(nameof(JwtSettings))
    .Get<JwtSettings>();

builder.Services.AddSingleton(jwtSettings);

var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
Console.WriteLine("[Program.cs] JWT SECRET: " + jwtSettings.Secret);
Console.WriteLine("[JWT SETTINGS]");
Console.WriteLine("Issuer: " + jwtSettings.Issuer);
Console.WriteLine("Audience: " + jwtSettings.Audience);
Console.WriteLine("Secret: " + jwtSettings.Secret);

// 🔐 JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();

// Auth Service
builder.Services.AddScoped<IAuthService, AuthService>();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("CoreApp.Application")));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(PromptTextCommandHandler).Assembly);
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.Load("CoreApp.Application"));

// Pipeline Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
// builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
