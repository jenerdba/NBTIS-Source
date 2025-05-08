using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using Microsoft.Net.Http.Headers;

namespace NBTIS.Core.Infrastructure
{
    public class IdentityCookieHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityCookieHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            foreach (var cookie in _httpContextAccessor!.HttpContext!.Request.Cookies)
            {
                request.Headers.Add("Cookie", new CookieHeaderValue(cookie.Key, cookie.Value).ToString());
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
