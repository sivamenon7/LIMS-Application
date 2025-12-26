using FluentValidation;
using LIMS.Common.Interfaces;
using LIMS.Database.Common.Context;
using MasterDataModule.Cache;
using MasterDataModule.DAL.Repositories;
using MasterDataModule.Services;
using MasterDataModule.Validators;
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

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "LIMS Master Data Module API",
        Version = "v1",
        Description = "Master Data module - Organisms, Types, Characterizations"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
    });

builder.Services.AddAuthorization();

// Infrastructure
builder.Services.AddScoped<IDbContext, DbContext>();
builder.Services.AddScoped<IUserContext, UserContext>();

// Redis cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "LIMS_MasterData_";
});

// Repositories
builder.Services.AddScoped<IOrganismRepository, OrganismRepository>();

// Cache services
builder.Services.AddScoped<IOrganismCacheService, OrganismCacheService>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrganismPayloadValidator>();

// Services
builder.Services.AddScoped<IOrganismService, OrganismService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("LIMSDatabase")!, name: "database")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

var app = builder.Build();

// Configure pipeline
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
    Log.Information("Starting LIMS Master Data Module API");
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
