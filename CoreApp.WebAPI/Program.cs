using System.Reflection;
using CoreApp.Application;
using CoreApp.Application.Common.Behaviors;
using CoreApp.Application.Common.Interfaces.Auth;
using CoreApp.Infrastructure.Auth;
using CoreApp.Infrastructure.Data;
using CoreApp.Infrastructure.Services;
using CoreApp.WebAPI.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------
// Services
// ------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreApp API", Version = "v1" });
});

builder.Services.AddHttpContextAccessor();

// Register EF Core DbContext
builder.Services.AddDbContext<CoreAppDbContext>(options =>
    options.UseInMemoryDatabase("CoreAppDb"));

// Bind JwtSettings from config
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// Register AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("CoreApp.Application")));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.Load("CoreApp.Application"));

// Pipeline Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
// builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>)); // requires IUnitOfWork

// Temporary Mock Auth Service
//builder.Services.AddScoped<IAuthService, MockAuthService>();

// ------------------------------------
// App Build
// ------------------------------------

var app = builder.Build();

// ------------------------------------
// Middleware
// ------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
