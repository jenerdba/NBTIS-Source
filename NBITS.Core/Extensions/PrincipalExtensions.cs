using NBTIS.Core.Infrastructure;
using System.Security.Claims;

namespace NBTIS.Core.Extensions
{
    public static class PrincipalExtensions
    {
        public static Claim? GetOktaClaimEmail(this ClaimsPrincipal principal) =>
            principal.FindFirst("email");

        public static Claim? GetOktaClaimSub(this ClaimsPrincipal principal) =>
            principal.FindFirst("sub");

        public static Claim? GetOktaClaimJti(this ClaimsPrincipal principal) =>
            principal.FindFirst("jti");

        public static Claim? GetOktaClaimName(this ClaimsPrincipal principal) =>
            principal.FindFirst("name");

        public static IEnumerable<Claim?> GetOktaClaimGroups(this ClaimsPrincipal principal) =>
            principal.Claims.Where(w => w.Type == "groups"
            && w.Value.StartsWith("NBTIS"));

        public static IEnumerable<UserPermission> GetUserPermissions(this ClaimsPrincipal principal)
        {
            var groups = GetOktaClaimGroups(principal)
                .Select(g => g?.Value)
                .Where(value => !string.IsNullOrEmpty(value))
                .Select(value => RoleMappings.ContainsKey(value!) ? RoleMappings[value!] : UserPermission.Public)
                .Distinct()
                .OrderBy(role => (int)role);

            return groups;
        }
        public static UserPermission GetUserPermission(this ClaimsPrincipal principal) =>
            principal.GetUserPermissions().FirstOrDefault();

        public static string GetUserType(this ClaimsPrincipal principal) =>
            $"{principal.GetOktaClaimEmail()?.Value} - {principal.GetUserPermission()}";

        public static readonly Dictionary<string, UserPermission> RoleMappings = new()
        {
            { "NBTIS Owner", UserPermission.Owner },
            { "NBTIS Admin", UserPermission.Admin },
            { "NBTIS Reviewer", UserPermission.Reviewer },
            { "NBTIS Submitter", UserPermission.Submitter },
            { "NBTIS Division Read Only", UserPermission.DivisionReadOnly }
        };
    }
}
