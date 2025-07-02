using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAspire.ApiService;
using TestAspire.ApiService.DataTransferObjects;
using TestAspire.ApiService.Entities;
using TestAspire.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();


builder.Services.AddAntiforgery();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<AutomapperProfile>(); });

// example of work around because some stuff are not prod ready, we have to disable tracing because some method are missing and called...
builder.AddNpgsqlDbContext<MyDbContext>("somedb", c => c.DisableTracing = true);
builder.Services.AddDbContextFactory<MyDbContext>();

builder.AddRedisClient("cache");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddRabbitMQClient("messaging");
builder.Services.AddTransient<ChannelFactory>();
builder.Services.AddTransient<ResultsPublisher>();
builder.Services.AddHostedService<ResultsConsumer>();
builder.Services.AddHostedService<AlgoInfoConsumer>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/datasets", async (IMapper autoMapper, MyDbContext db) =>
{
    var datasetsInDb = await db.Datasets.ToListAsync();
    var mappedDatasets = autoMapper.Map<IEnumerable<DatasetReadDto>>(datasetsInDb);
    return mappedDatasets;
});
app.MapGet("/datasets/{id}", async (int id, IMapper autoMapper, MyDbContext db) =>
{
    var datasetInDb = await db.Datasets.FindAsync(id);
    if (datasetInDb is null) return Results.NotFound();

    var mappedDataset = autoMapper.Map<DatasetReadDto>(datasetInDb);
    return Results.Ok(mappedDataset);
});
app.MapDelete("/datasets/{id}", async (int id, MyDbContext db) =>
{
    var datasetInDb = await db.Datasets.FindAsync(id);
    if (datasetInDb is null) return Results.NotFound();

    db.Remove(datasetInDb);
    await db.SaveChangesAsync();

    return Results.Ok();
});
app.MapPost("/datasets", async ([FromForm] DatasetDto dataset, IMapper autoMapper, MyDbContext db) =>
{
    var datasetForDb = autoMapper.Map<Dataset>(dataset);
    db.Datasets.Update(datasetForDb);
    await db.SaveChangesAsync();
    return Results.Created($"/datasets/{dataset.Id}", autoMapper.Map<DatasetDto>(datasetForDb));
}).DisableAntiforgery();

app.MapGet("/algos", async (IMapper autoMapper, MyDbContext db) =>
{
    var algorithmsInDb = await db.Algos.ToListAsync();
    var mappedAlgorithms = autoMapper.Map<IEnumerable<AlgoDto>>(algorithmsInDb);
    return mappedAlgorithms;
});
app.MapGet("/algos/{id}", async (int id, IMapper autoMapper, MyDbContext db) =>
{
    var algoInDb = await db.Algos.FindAsync(id);
    if (algoInDb is null) return Results.NotFound(id);

    var mappedAlgo = autoMapper.Map<AlgoDto>(algoInDb);
    return Results.Ok(mappedAlgo);
});
app.MapDelete("/algos/{id}", async (int id, MyDbContext db) =>
{
    var algoInDb = await db.Algos.FindAsync(id);
    if (algoInDb is null) return Results.NotFound(id);

    db.Remove(algoInDb);
    await db.SaveChangesAsync();

    return Results.Ok();
});
app.MapPost("/algos", async (AlgoDto algo, IMapper autoMapper, MyDbContext db) =>
{
    var algoForDb = autoMapper.Map<Algo>(algo);
    algoForDb.IsAlive = false;
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
    if (resultInDb is null) return Results.NotFound(id);

    var mappedResult = autoMapper.Map<ResultDto>(resultInDb);
    return Results.Ok(mappedResult);
});
app.MapDelete("/results/{id}", async (int id, MyDbContext db) =>
{
    var resultInDb = await db.Results.FindAsync(id);
    if (resultInDb is null) return Results.NotFound(id);

    db.Remove(resultInDb);
    await db.SaveChangesAsync();

    return Results.Ok();
});
app.MapPost("/results",
    async (ResultWriteDto result, MyDbContext db, IMapper autoMapper, ResultsPublisher publisher) =>
    {
        var algo = await db.Algos.FindAsync(result.AlgoId);
        var dataset = await db.Datasets.FindAsync(result.DatasetId);

        if (algo is null || dataset is null)
            return Results.NotFound(
                $"At one one of those element was not found: algo ({result.AlgoId}) or dataset ({result.DatasetId})");

        var createdResult = new Result { Algo = algo, Dataset = dataset, DatasetId = dataset.Id, AlgoId = algo.Id };
        var resultEntity = db.Results.Add(createdResult);
        await db.SaveChangesAsync();

        var mappedResult = autoMapper.Map<ResultDto>(resultEntity.Entity);
        publisher.Send(mappedResult);

        return Results.Created($"/algos/{resultEntity.Entity.Id}", resultEntity.Entity);
    });

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    db.Database.Migrate();
    await db.Algos.ForEachAsync(x => x.IsAlive = false);
    await db.SaveChangesAsync();
}

app.Run();