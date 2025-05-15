using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Data;
using TestAspire.ApiService;
using TestAspire.ApiService.DataTransferObjects;
using TestAspire.ApiService.Entities;
using TestAspire.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutomapperProfile>());

// example of work around because some stuff are not prod ready, we have to disable tracing because some method are missing and called...
builder.AddNpgsqlDbContext<MyDbContext>("somedb", c => c.DisableTracing = true);
builder.Services.AddDbContextFactory<MyDbContext>();

builder.AddRedisClient("cache");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddRabbitMQClient("messaging");
builder.Services.AddTransient<ChannelFactory>();
builder.Services.AddTransient<ResultsPublisherService>();
builder.Services.AddHostedService<ResultsConsumerService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

string[] summaries =
    ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapGet("/datasets", async (IMapper autoMapper, MyDbContext db) =>
{
    var datasetsInDb = await db.Datasets.ToListAsync();
    var mappedDatasets = autoMapper.Map<IEnumerable<DatasetDto>>(datasetsInDb);
    return mappedDatasets;
});
app.MapGet("/datasets/{id}", async (int id, IMapper autoMapper, MyDbContext db) =>
{
    var datasetInDb = await db.Datasets.FindAsync(id);
    if (datasetInDb is null)
    {
        return Results.NotFound();
    }

    var mappedDataset = autoMapper.Map<DatasetDto>(datasetInDb);
    return Results.Ok(mappedDataset);
});
app.MapPost("/datasets", async (DatasetDto dataset, IMapper autoMapper, MyDbContext db) =>
{
    var datasetForDb = autoMapper.Map<Dataset>(dataset);
    db.Datasets.Update(datasetForDb);
    await db.SaveChangesAsync();
    return Results.Created($"/datasets/{dataset.Id}", autoMapper.Map<DatasetDto>(datasetForDb));
});

app.MapGet("/algos", async (IMapper autoMapper, MyDbContext db) =>
{
    var algorithmsInDb = await db.Algos.ToListAsync();
    var mappedAlgorithms = autoMapper.Map<IEnumerable<AlgoDto>>(algorithmsInDb);
    return mappedAlgorithms;
});
app.MapGet("/algos/{id}", async (int id, IMapper autoMapper, MyDbContext db) =>
{
    var algoInDb = await db.Algos.FindAsync(id);
    if (algoInDb is null)
    {
        return Results.NotFound(id);
    }

    var mappedAlgo = autoMapper.Map<AlgoDto>(algoInDb);
    return Results.Ok(mappedAlgo);
});
app.MapPost("/algos", async (AlgoDto algo, IMapper autoMapper, MyDbContext db) =>
{
    var algoForDb = autoMapper.Map<Algo>(algo);
    db.Algos.Update(algoForDb);
    await db.SaveChangesAsync();
    return Results.Created($"/algos/{algoForDb.Id}", autoMapper.Map<AlgoDto>(algoForDb));
});

app.MapGet("/results", async (IMapper autoMapper, MyDbContext db) =>
{
    var resultsInDb = await db.Results.Include(result => result.Algo).Include(result => result.Dataset).ToListAsync();
    var mappedResults = autoMapper.Map<IEnumerable<ResultDto>>(resultsInDb);
    return Results.Ok(mappedResults);
});

app.MapGet("/results/{id}", async (int id, IMapper autoMapper, MyDbContext db) =>
{
    var resultInDb = await db.Results.FindAsync(id);
    if (resultInDb is null)
    {
        return Results.NotFound(id);
    }

    var mappedResult = autoMapper.Map<ResultDto>(resultInDb);
    return Results.Ok(mappedResult);
});
app.MapPost("/results",
    async (ResultDto result, MyDbContext db, IMapper autoMapper, ResultsPublisherService publisher) =>
    {
        var resultForDb = autoMapper.Map<Result>(result);
        db.Results.Update(resultForDb);
        await db.SaveChangesAsync();
        var mappedResult = autoMapper.Map<ResultDto>(resultForDb);
        publisher.Send(mappedResult);

        // Input: contains an algo and a dataset
        // we need to fetch them if they exist and then trigger the algo by using the databus
        // TODO: post in the bus to run an algo
        // option1: do the calculation and wait within the endpoint
        // option2: return OK and do calculation in the background
        return Results.Created($"/algos/{mappedResult.Id}", mappedResult);
    });


app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    db.Database.Migrate();
}

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}