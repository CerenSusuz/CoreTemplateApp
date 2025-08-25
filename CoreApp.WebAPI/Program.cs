using Core.AI.Abstractions;
using Core.AI.Commands;
using Core.AI.Config;
using Core.AI.FunctionCalling;
using Core.AI.Memory;
using Core.AI.Providers;
using Core.AI.Providers.Ollama;
using Core.AI.Providers.OpenRouter;
using Core.AI.Providers.Profiles;
using Core.AI.Providers.SemanticKernel;
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
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Controllers ----------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// ---------------- Configuration Binding ----------------
builder.Services.Configure<AISettings>(builder.Configuration.GetSection("AiSettings"));
builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection("Ollama"));
builder.Services.Configure<OpenRouterSettings>(builder.Configuration.GetSection("OpenRouter"));

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<AISettings>>().Value);
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<OllamaSettings>>().Value);
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<OpenRouterSettings>>().Value);

// ---------------- AI Services ----------------
builder.Services.AddHttpClient<OllamaAiService>();
builder.Services.AddScoped<OpenRouterAiService>();
builder.Services.AddScoped<IAIService, AIServiceResolver>();
builder.Services.AddSingleton<AgentProfileProvider>();

// Model Providers
builder.Services.AddScoped<OllamaModelProvider>();
builder.Services.AddScoped<OpenRouterModelProvider>();
builder.Services.AddScoped<AIModelProviderResolver>();

// Agent Service
builder.Services.AddScoped<IAgentService, SemanticKernelAgentService>();
builder.Services.AddSingleton<ChatHistoryStore>();
builder.Services.AddSingleton<AgentProfileProvider>();

// Function Calling Core
builder.Services.AddScoped<AiFunctionDispatcher>();
builder.Services.AddSingleton<IFunctionRegistry, InMemoryFunctionRegistry>();

// Register all IAiFunction implementations automatically
builder.Services.Scan(scan => scan
    .FromAssemblies(Assembly.Load("Core.AI"))
    .AddClasses(c => c.AssignableTo<IAiFunction>())
    .AsImplementedInterfaces()
    .WithSingletonLifetime());

// ---------------- Database ----------------
builder.Services.AddDbContext<CoreAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------- Authentication / Authorization ----------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);

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

// ---------------- Application Services ----------------
builder.Services.AddScoped<IAuthService, AuthService>();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("CoreApp.Application")));
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(PromptTextCommandHandler).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.Load("CoreApp.Application"));

// Pipeline Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
// builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

// ---------------- Swagger ----------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreApp API", Version = "v1" });

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
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ---------------- App Pipeline ----------------
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
