using MyProject;
using MyProject.Components;
using MyProject.Infrastructure;
using MyProject.Infrastructure.Auth;
using MyProject.Infrastructure.Database;
using MyProject.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDatabase(connectionString);
builder.Services.AddInfrastructure(config);
builder.Services.AddFluentUIComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(AuthConstants.TwoFactorCookieName);

builder.Services.AddScoped<AuthenticationStateProvider, AppAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

await Storage.InitAsync(config);

////
//// Create a test user
////
//var userRepository = app.Services.GetRequiredService<UserRepository>();
//var dbFactory = app.Services.GetRequiredService<IDbConnectionFactory>();
//using var connection = await dbFactory.CreateConnectionAsync();
//User user = new User
//{
//    AccountType = AccountType.Internal,
//    Username = "test@test.de",
//    Password = "12Tester34#",
//    DisplayName = "Testuser",
//    IsActive = true,
//    Email = "test@test.de",
//    Salt = SaltGenerator.GenerateSaltBase64()
//};

//PasswordHasher<User> hasher = new();
//string passwordHashed = hasher.HashPassword(user, user.Password + user.Salt);
//user.Password = passwordHashed;
//await userRepository.CreateAsync(user, connection);

////
//// End create test user
////

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var accountGroup = app.MapGroup("/Account");

accountGroup.MapGet("/Logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("/Account/Login");
});

app.Run();
