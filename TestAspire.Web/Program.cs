using TestAspire.Web.Api;
using TestAspire.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<AlgoClient>(client => { client.BaseAddress = new Uri("https+http://apiservice"); });
builder.Services.AddHttpClient<DatasetClient>(client => { client.BaseAddress = new Uri("https+http://apiservice"); });
builder.Services.AddHttpClient<ResultClient>(client => { client.BaseAddress = new Uri("https+http://apiservice"); });

builder.AddRedisClient("cache");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();