using AsM.Components;
using AsM.Data;
using AsM.Interfaces;
using AsM.Models;
using AsM.Services;
using Cassandra.Mapping;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Register data contexts
builder.Services.AddScoped<ICassandraDbContext, CassandraDbContext>();
builder.Services.AddScoped<INeo4jDbContext, Neo4jDbContext>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddControllers();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "auth";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.LoginPath = "/Account/Signin";
    options.LogoutPath = "/Account/Signout";
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/Error";
});
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

MappingConfiguration.Global.Define(new Map<User>().TableName("users").PartitionKey(u => u.Id));


try
{
    app.Run();
}
catch (Exception e)
{
    if (e.Message == "Unrecoverable")
    {
        Log.Fatal("Unrecoverable error. Exiting."); 
        Environment.Exit(1);
    }
}
finally
{
    await Log.CloseAndFlushAsync();
}
