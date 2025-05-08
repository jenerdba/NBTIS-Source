using Microsoft.AspNetCore.Http;
using NBTIS.Core.Extensions;
using System.Security.Claims;

public interface ICurrentUserService
{
    string UserId { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId => _httpContextAccessor.HttpContext?.User.GetOktaClaimEmail()?.Value ?? "unknown@example.com";
}