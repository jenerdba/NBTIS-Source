using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoapCore;
using EmailService.Services;
using EmailService.Extensions;
using Microsoft.Extensions.Logging;
using EmailService.Settings;

public static class ServiceRegistration
{
    public static void AddEmailService(this IServiceCollection services, IConfiguration configuration)
    {

        services.RegisterSettings(configuration);

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConfiguration(configuration.GetSection("Logging"));
            logging.AddConsole();
            logging.AddDebug();
        });

        services.AddScoped<IEmailSoapService, EmailSoapService>();
        services.AddScoped<IEmailGraphService, EmailGraphService>();
        services.AddScoped<IEmailManagerService, EmailManagerService>();
        services.AddScoped(sp =>
{
            var config = sp.GetRequiredService<IConfiguration>();
            var settings = new GraphSettings();
            config.GetSection("Graph").Bind(settings);
            return settings;
        });
        
    }
}
