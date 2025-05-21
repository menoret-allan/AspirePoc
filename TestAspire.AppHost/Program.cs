using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithRedisInsight();

var username = builder.AddParameter("RabbitMqAdminUsername", secret: true);
var password = builder.AddParameter("RabbitMqAdminPassword", secret: true);

var rabbitmq = builder.AddRabbitMQ("messaging", username, password)
    .WithManagementPlugin();


var postgres = builder
    .AddPostgres("postgres")
    .PublishAsAzurePostgresFlexibleServer()
    .WithPgAdmin()
    .WithDataVolume(isReadOnly: false);

var databaseName = "somedb";
var creationScript = $$"""
                       -- Create the database
                       CREATE DATABASE {{databaseName}};

                       """;

var db = postgres.AddDatabase(databaseName)
    .WithCreationScript(creationScript);

var algorithmDummy = builder.AddProject<TestAspire_AlgorithmDummy>("algorithmdummy")
    .WithReference(rabbitmq).WaitFor(rabbitmq);

var apiService = builder.AddProject<TestAspire_ApiService>("apiservice")
    .WithReference(db).WaitFor(db)
    .WaitFor(algorithmDummy)
    .WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithReference(cache);

builder.AddProject<TestAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(cache)
    .WaitFor(apiService);

builder.Build().Run();