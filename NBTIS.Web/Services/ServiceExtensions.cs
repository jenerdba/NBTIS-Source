using NBTIS.Core.Services;
using NBTIS.Web.Mapping;
using NBTIS.Core.Mapping;

namespace NBTIS.Web.Services
{
    public static class ServiceExtensions
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<LocationService>();
            services.AddScoped<AnnouncementsService>();
            services.AddScoped<FileValidationService>();

            services.AddScoped<DataProcessor>();         
            services.AddSingleton<IDuplicateChecker, DuplicateChecker>();
            services.AddScoped<SubmittalService>();
            services.AddScoped<AdministrationService>();
            services.AddScoped<DataCorrectionService>();
            services.AddScoped<LookupDataItemService>();

            services.AddScoped<IRulesService>(sp =>
            {
                var coreAssemblyPath = Path.GetDirectoryName(typeof(RulesService).Assembly.Location);
                var workflowsPath = Path.Combine(coreAssemblyPath, "Workflows");
                return new RulesService(workflowsPath);
            });
            services.AddScoped<LOVValidatorFactory>();
            services.AddScoped<StateValidatorService>();
            services.AddScoped<FedAgencyValidatorService>();
            services.AddScoped<BridgeStagingLoaderService>();
            services.AddScoped<SNBISanitizer>();

            services.AddAutoMapper(typeof(MapSubmittalLog));
            services.AddAutoMapper(typeof(MapStageBridge));

            services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        }
    }

}
