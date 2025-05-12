using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithRedisInsight();

var postgres = builder
    .AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume(isReadOnly: false);

var databaseName = "somedb";
var creationScript = $$"""
                       -- Create the database
                       CREATE DATABASE {{databaseName}};

                       """;

var db = postgres.AddDatabase(databaseName)
    .WithCreationScript(creationScript);

var apiService = builder.AddProject<TestAspire_ApiService>("apiservice")
    .WithReference(db).WaitFor(db)
    .WithReference(cache);

builder.AddProject<TestAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(cache)
    .WaitFor(apiService);

builder.Build().Run();