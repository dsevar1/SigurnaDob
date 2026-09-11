using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using SigurnaDob.App.Auth;
using SigurnaDob.App.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options => options.DetailedErrors = true);
}

builder.Services.AddMudServices();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddHttpClient("SigurnaDobApi", client =>
{
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
        ?? throw new InvalidOperationException("ApiBaseUrl nije konfiguriran u appsettings.json.");
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Scoped (jedna instanca po circuitu) - Authorization header postavlja JwtAuthenticationStateProvider
// izravno na ovaj HttpClient, ne kroz DelegatingHandler (vidi komentar u JwtAuthenticationStateProvider).
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SigurnaDobApi"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
