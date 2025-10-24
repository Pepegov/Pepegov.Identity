using MudBlazor.Services;
using Pepegov.Identity.Client.WASM.Client.Pages;
using Pepegov.Identity.Client.WASM.Components;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

builder.Services.AddMsalAuthentication(options =>
{
    var providerOptions = options.ProviderOptions;
    providerOptions.Authentication.Authority = "https://localhost:5001"; // URL вашего OpenIddict
    providerOptions.Authentication.ClientId = "blazor-client";
    providerOptions.Authentication.ResponseType = "code"; // Используется Authorization Code Flow
    providerOptions.DefaultAccessTokenScopes.Add("profile");
    providerOptions.DefaultAccessTokenScopes.Add("email");

    providerOptions.Authentication.RedirectUri = "https://localhost:5002/authentication/login-callback";
    providerOptions.Authentication.PostLogoutRedirectUri = "https://localhost:5002/";
});


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Pepegov.Identity.Client.WASM.Client._Imports).Assembly);

app.Run();