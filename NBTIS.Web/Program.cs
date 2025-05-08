using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using NBTIS.Web.Components;
using NBTIS.Core.Extensions;
using NBTIS.Core.Infrastructure;
using Okta.AspNetCore;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using NBTIS.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using NBTIS.Web.Services;
using Microsoft.EntityFrameworkCore;
using NBTIS.Data.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using NBTIS.Core.Utilities;
using NBTIS.Core.Services;
using NBTIS.Web.Hubs;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using NBTIS.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddCustomAuthentication();

var environment = builder.Environment.EnvironmentName;

// Configure Serilog to read from the configuration (appsettings.json)
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

// Configure services
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Action<SqlServerDbContextOptionsBuilder> sqlServerOptions = sql =>
{
    sql.CommandTimeout(300);
    sql.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null);
    sql.MaxBatchSize(100);
};

// Register the "normal" scoped DbContext for per?request DI
builder.Services.AddDbContext<DataContext>(
    options => options.UseSqlServer(connectionString, sqlServerOptions),
    contextLifetime: ServiceLifetime.Scoped,
    optionsLifetime: ServiceLifetime.Singleton
);
//Register the factory (singleton by default) for long running background tasks.
builder.Services.AddDbContextFactory<DataContext>(options =>
{
    options.UseSqlServer(connectionString, sqlServerOptions);           
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IdentityCookieHandler>();

builder.Services.AddSingleton(new JsonSerializerOptions
{
    Converters = {
        new CustomStringJsonConverter(),
        new CustomNullableDoubleJsonConverter()
    },
    PropertyNameCaseInsensitive = true
});


var appSettings = builder.Configuration.GetSection("App").Get<AppSettings>()!;
builder.Services.AddHttpClient<HttpClientService>(options =>
{
    options.BaseAddress = new Uri(appSettings.ApiUrl);

}).AddHttpMessageHandler<IdentityCookieHandler>();

builder.Services.RegisterServices();
builder.Services.AddSignalR();
builder.Services.AddScoped<IProgressNotifier, SignalRProgressNotifier>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddTelerikBlazor();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddEmailService(builder.Configuration);

var app = builder.Build();

// Resolve DataContext from the app's services. This is needed for Static CustomRules() class.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    CustomRules.Initialize(context);

    // Preload necessary lookup validators so that their data is cached.
    var statesValidator = LOVValidatorsService.GetValidator(context, "LookupStates");
    var countiesValidator = LOVValidatorsService.GetValidator(context, "LookupCounties");
    var ownersValidator = LOVValidatorsService.GetValidator(context, "LookupOwners");
    var valuesValidator = LOVValidatorsService.GetValidator(context, "LookupValues");

    // (Optionally, call methods on them to force data loading and cache population.)
    bool testState = statesValidator.IsValidStateCode("CA");
    bool testCounty = countiesValidator.IsValidCountyCode("CA", "001");
    bool testOwner = ownersValidator.IsValidOwnerCode("123");
    bool testValues = valuesValidator.IsValidCode("BW03", "123");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapLoginAndLogout();

//SignalR Hub for Progress Update.
app.MapHub<ProgressHub>("/progressHub");

app.Run();

