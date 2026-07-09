using PerformanceLab.Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Typed HttpClient that calls the McpServer REST API
builder.Services.AddHttpClient<McpApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["McpServerUrl"] ?? "http://localhost:5200");
    client.Timeout     = TimeSpan.FromSeconds(60);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<PerformanceLab.Dashboard.Components.App>()
   .AddInteractiveServerRenderMode();

app.Run();
