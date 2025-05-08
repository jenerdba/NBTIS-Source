using EmailService.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace EmailService.Extensions
{
    public static class RegistrationExtensions
    {
        public static void RegisterSettings(this IServiceCollection services, IConfiguration config)
        {
            services.RegisterSetting<GraphSettings>("Graph", config);
            services.RegisterSetting<AppSettings>("App", config);
        }

        public static void RegisterSetting<TSetting>(this IServiceCollection services, string sectionName, IConfiguration config)
            where TSetting : class, new()
        {
            services.Configure<TSetting>(config.GetSection(sectionName));
            services.AddScoped(s => s.GetRequiredService<IOptionsSnapshot<TSetting>>().Value);
        }
    }
}

