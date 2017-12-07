using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Developers.Italia.SPID.Middlewares
{
    public class SpidSamlMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly SpidMiddlewareOptions _options;

        public SpidSamlMiddleware(RequestDelegate next, IOptions<SpidMiddlewareOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        public Task Invoke(HttpContext context)
        {
            var cultureQuery = context.Request.Query["culture"];


            // Call the next delegate/middleware in the pipeline
            return this._next(context);
        }

    }

    public class SpidMiddlewareOptions
    {
        public SpidMiddlewareOptions()
        {
            Providers = new List<SpidIdentityProvider>();
        }

        public void LoadFromJson(string jsonFile)
        {


        }
        public List<SpidIdentityProvider> Providers { get; set; }
    }


    public class SpidIdentityProvider
    {

        public string IdentityProviderId { get; set; }
        public string IdentityProviderRequestMethod { get; set; }
        public string IdentityProviderLoginUrl { get; set; }
        public string IdentityProviderLogoutUrl { get; set; }

        public string ServiceProviderId { get; set; }
        public string ServiceProviderCert { get; set; }
        public string ServiceProviderPrivateKey { get; set; }
        public string ServiceProviderPrivateKeyPassword { get; set; }



    }


    public static class SpidMiddlewareExtensions
    {
        public static IApplicationBuilder UseSpidMiddleware(
            this IApplicationBuilder builder, SpidMiddlewareOptions options)
        {
            return builder.UseMiddleware<SpidMiddleware>(Options.Create(options));
        }
    }
}