using Blazor.WebApp;
using Blazor.WebApp.Components;
using Blazored.LocalStorage;
using Blazored.Toast;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClientHelper>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<StateContainer>();
builder.Services.AddScoped<NavigationService>();
builder.Services.AddBlazoredToast();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
