using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NBTIS.Core.Extensions;

namespace NBTIS.Core.Infrastructure
{
    public enum UserPermission
    {
        Owner = 1,
        Admin = 2,
        Reviewer = 3,
        Submitter = 4,
        DivisionReadOnly = 5,
        Public = 6
    }

    public class CustomAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly UserPermission[] _userPermissions;

        public CustomAuthorizationFilter(UserPermission[] userPermissions)
        {
            _userPermissions = userPermissions;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await Task.Yield();

            var userPermission = context.HttpContext.User.GetUserPermission();

            if (!_userPermissions.Contains(userPermission))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }

    public abstract class BaseAuthorizeAttribute : TypeFilterAttribute
    {
        protected BaseAuthorizeAttribute(params UserPermission[] permissions)
            : base(typeof(CustomAuthorizationFilter))
        {
            Arguments = new object[] { permissions };
        }
    }

    public class AuthorizeOwnerAttribute() : 
        BaseAuthorizeAttribute(
            UserPermission.Owner);

    public class AuthorizeAdminAttribute() : 
        BaseAuthorizeAttribute(
            UserPermission.Admin, 
            UserPermission.Owner);

    public class AuthorizeReviewerAttribute() : 
        BaseAuthorizeAttribute(
            UserPermission.Admin, 
            UserPermission.Owner, 
            UserPermission.Reviewer);

    public class AuthorizeSubmitterAttribute() : 
        BaseAuthorizeAttribute(
            UserPermission.Admin, 
            UserPermission.Owner, 
            UserPermission.Reviewer, 
            UserPermission.Submitter);

    public class AuthorizeDivisionReadOnlyAttribute() : 
        BaseAuthorizeAttribute(
            UserPermission.Admin, 
            UserPermission.Owner, 
            UserPermission.Reviewer, 
            UserPermission.Submitter, 
            UserPermission.DivisionReadOnly);
}
