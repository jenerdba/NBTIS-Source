using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBTIS.Core.Infrastructure;
using NBTIS.Core.Settings;
using Okta.AspNetCore;

namespace NBTIS.Core.Extensions
{
    public static class WebAppExtensions
    {
        public static void AddCustomAuthentication(this WebApplicationBuilder builder)
        {
            var authenticationBuilder = builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(c =>
            {
                c.Cookie.Name = "NBTIS";
                c.Cookie.HttpOnly = true;
                c.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                c.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                c.EventsType = typeof(CustomCookieAuthenticationEvents);
                c.Cookie.MaxAge = TimeSpan.FromMinutes(15);
                c.Cookie.SameSite = SameSiteMode.None;
            });

            var oktaSettings = builder.Configuration.GetSection("Okta").Get<OktaSettings>();

            if (oktaSettings != null)
            {
                authenticationBuilder.AddOktaMvc(new OktaMvcOptions
                {
                    OktaDomain = oktaSettings.OktaDomain,
                    AuthorizationServerId = oktaSettings.AuthorizationServerId,
                    ClientId = oktaSettings.ClientId,
                    ClientSecret = oktaSettings.ClientSecret,
                    Scope = new List<string> { "openid", "profile", "email" }
                });
            }           

            var appSettings = builder.Configuration.GetSection("App").Get<AppSettings>()!;
            var keyPath = appSettings.DataProtectionKeyPath;

            if (!Directory.Exists(keyPath))
            {
                Directory.CreateDirectory(keyPath);
            }

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
                .SetApplicationName(appSettings.Name);

            builder.Services.AddScoped<CustomCookieAuthenticationEvents>();
        }

        public static IEndpointConventionBuilder MapLoginAndLogout(this WebApplication app)
        {
            var group = app.MapGroup("/authentication");

            group.MapGet("/login", (string? returnUrl) => TypedResults.Challenge(GetAuthProperties(returnUrl)))
                .AllowAnonymous();

            group.MapGet("/logout", (string? returnUrl) => TypedResults.SignOut(GetAuthProperties(returnUrl),
                [CookieAuthenticationDefaults.AuthenticationScheme]));

            return group;
        }

        private static AuthenticationProperties GetAuthProperties(string? returnUrl)
        {
            const string pathBase = "/";

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = pathBase;
            }
            else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
            {
                returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
            }
            else if (returnUrl[0] != '/')
            {
                returnUrl = $"{pathBase}{returnUrl}";
            }

            return new AuthenticationProperties { RedirectUri = returnUrl };
        }
    }
}
