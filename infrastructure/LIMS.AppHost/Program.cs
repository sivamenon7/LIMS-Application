var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithDataVolume()
    .AddDatabase("limsdb");

// Add Redis cache
var redis = builder.AddRedis("redis")
    .WithDataVolume();

// Add RabbitMQ for event bus
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithDataVolume()
    .WithManagementPlugin();

// Add the LIMS API
var api = builder.AddProject<Projects.LIMS_API>("lims-api")
    .WithReference(sqlServer)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithEnvironment("ConnectionStrings__LIMSDatabase", sqlServer)
    .WithEnvironment("Redis__ConnectionString", redis)
    .WithEnvironment("RabbitMQ__Host", rabbitmq);

// Add the Blazor WASM frontend
builder.AddProject<Projects.LIMS_UI_Blazor>("lims-ui")
    .WithReference(api)
    .WithEnvironment("ApiBaseUrl", api);

builder.Build().Run();
