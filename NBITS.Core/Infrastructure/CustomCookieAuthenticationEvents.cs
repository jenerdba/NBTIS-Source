using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using NBTIS.Core.Extensions;
using PrincipalExtensions = NBTIS.Core.Extensions.PrincipalExtensions;

namespace NBTIS.Core.Infrastructure
{
    public class CustomCookieAuthenticationEvents() : CookieAuthenticationEvents
    {
        public override Task SigningIn(CookieSigningInContext context)
        {
            var principal = context.Principal;

            if (principal == null)
            {
                throw new ApplicationException("Missing user principal.");
            }

            var selectedClaims = new List<Claim?>()
    {
        principal.GetOktaClaimEmail(),
        principal.GetOktaClaimSub(),
        principal.GetOktaClaimJti(),
        principal.GetOktaClaimName()
    };

            // **NEW: Capture group claims into a list before modifying the claims collection**
            var oktaGroupClaims = principal.GetOktaClaimGroups().ToList();

            selectedClaims.AddRange(oktaGroupClaims);

            if (selectedClaims.Any(a => a == null))
            {
                throw new ApplicationException($"User {principal.Identity?.Name} is missing claims.");
            }

            var identity = principal.Identity as ClaimsIdentity;

            if (identity == null)
            {
                throw new ApplicationException("Missing user identity.");
            }

            var claims = identity.Claims.ToList();

            foreach (var claim in claims)
            {
                identity.RemoveClaim(claim);
            }

            identity.AddClaims(selectedClaims.Where(w => w != null).Cast<Claim>());

            // **NEW: Use the captured group claims to add role claims**
            foreach (var groupClaim in oktaGroupClaims)
            {
                if (groupClaim?.Value is not null &&
                    PrincipalExtensions.RoleMappings.TryGetValue(groupClaim.Value, out var permission))
                {
                    // Add a role claim using the string representation of the permission (e.g., "Reviewer")
                    identity.AddClaim(new Claim(ClaimTypes.Role, permission.ToString()));
                }
            }

            return base.SigningIn(context);
        }

    }
}
