using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// example of work around because some stuff are not prod ready
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
    // TODO: post in the bus to run an algo
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

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Algo> Algos { get; set; }
    public DbSet<Dataset> Datasets { get; set; }
    public DbSet<Result> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dataset>()
            .HasMany(e => e.Results)
            .WithOne(e => e.Dataset)
            .HasForeignKey(e => e.DatasetId)
            .HasPrincipalKey(e => e.Id);
        modelBuilder.Entity<Algo>()
            .HasMany(e => e.Results)
            .WithOne(e => e.Algo)
            .HasForeignKey(e => e.AlgoId)
            .HasPrincipalKey(e => e.Id);
    }
}

public class Result
{
    public int Id { get; set; }
    public int DatasetId { get; set; }
    public required Dataset Dataset { get; set; }
    public int AlgoId { get; set; }
    public required Algo Algo { get; set; }
    public string? ResultJson { get; set; }
}

public class Dataset
{
    public int Id { get; set; }
    public required string Image { get; set; }
    public required string Name { get; set; }
    public ICollection<Result> Results { get; set; }
}

public class Algo
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Version { get; set; }
    public ICollection<Result> Results { get; set; }
}