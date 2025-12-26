using FluentValidation;
using LIMS.MasterData.API.Cache;
using LIMS.MasterData.API.DAL.Repositories;
using LIMS.MasterData.API.Services;
using LIMS.MasterData.API.Validators;
using LIMS.Shared.Core.Interfaces;
using LIMS.Shared.Infrastructure.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "LIMS Master Data API",
        Version = "v1",
        Description = "Master Data module for LIMS - Organisms, Customers, Users"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
    });

builder.Services.AddAuthorization();

// Add infrastructure services
builder.Services.AddScoped<IDbContext, DbContext>();
builder.Services.AddScoped<IUserContext, UserContext>();

// Add Redis cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "LIMS_MasterData_";
});

// Add repositories
builder.Services.AddScoped<IOrganismRepository, OrganismRepository>();

// Add cache services
builder.Services.AddScoped<IOrganismCacheService, OrganismCacheService>();

// Add validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrganismPayloadValidator>();

// Add services
builder.Services.AddScoped<IOrganismService, OrganismService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("LIMSDatabase")!, name: "database")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting LIMS Master Data API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
