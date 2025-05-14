using Microsoft.EntityFrameworkCore;
using TestAspire.ApiService.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// example of work around because some stuff are not prod ready, we have to disable tracing because some method are missing and called...
builder.AddNpgsqlDbContext<MyDbContext>("somedb", c => c.DisableTracing = true);

builder.AddRedisClient("cache");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapGet("/datasets", async (MyDbContext db) => await db.Datasets.ToListAsync());
app.MapGet("/datasets/{id}", async (int id, MyDbContext db) => await db.Datasets.FindAsync(id)
    is { } dataset
    ? Results.Ok(dataset)
    : Results.NotFound());
app.MapPost("/datasets", async (Dataset dataset, MyDbContext db) =>
{
    db.Datasets.Add(dataset);
    await db.SaveChangesAsync();
    return Results.Created($"/datasets/{dataset.Id}", dataset);
});

app.MapGet("/algos", async (MyDbContext db) => await db.Algos.ToListAsync());
app.MapGet("/algos/{id}", async (int id, MyDbContext db) => await db.Algos.FindAsync(id)
    is { } algo
    ? Results.Ok(algo)
    : Results.NotFound());
app.MapPost("/algos", async (Algo algo, MyDbContext db) =>
{
    db.Algos.Add(algo);
    await db.SaveChangesAsync();
    return Results.Created($"/algos/{algo.Id}", algo);
});

app.MapGet("/results", async (MyDbContext db) => await db.Results.ToListAsync());
app.MapGet("/results/{id}", async (int id, MyDbContext db) => await db.Results.FindAsync(id)
    is { } result
    ? Results.Ok(result)
    : Results.NotFound());
app.MapPost("/results", (Result result, MyDbContext db) =>
{
    // Input: contains an algo and a dataset
    // we need to fetch them if they exist and then trigger the algo by using the databus
    // TODO: post in the bus to run an algo
    // option1: do the calculation and wait within the endpoint
    // option2: return OK and do calculation in the background
    return Results.Ok();
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