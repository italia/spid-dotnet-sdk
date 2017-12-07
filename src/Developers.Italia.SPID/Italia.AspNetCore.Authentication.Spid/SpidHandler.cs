using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Developers.Italia.SPID.SAML;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Italia.AspNetCore.Authentication.Spid
{
    public class SpidHandler : RemoteAuthenticationHandler<SpidOptions>
    {
        private const string UriSchemeDelimiter = "://";

        private const string InputTagFormat = @"<input type=""hidden"" name=""{0}"" value=""{1}"" />";
        private const string HtmlFormFormat = @"<!doctype html>
<html>
<head>
    <title>Please wait while you're being redirected to the identity provider</title>
</head>
<body>
    <form name=""form"" method=""post"" action=""{0}"">
        {1}
        <noscript>Click here to finish the process: <input type=""submit"" /></noscript>
    </form>
    <script>document.form.submit();</script>
</body>
</html>";

        protected HtmlEncoder HtmlEncoder { get; }

        public SpidHandler(IOptionsMonitor<SpidOptions> options, ILoggerFactory logger, HtmlEncoder htmlEncoder, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            HtmlEncoder = htmlEncoder;
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {

            // order for local RedirectUri
            // 1. challenge.Properties.RedirectUri
            // 2. CurrentUri if RedirectUri is not set)
            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = CurrentUri;
            }


            //string returnUrl = "/";

            //if (!string.IsNullOrEmpty(HttpContext.Request.Query["redirectUrl"]))
            //{
            //    returnUrl = HttpContext.Request.Query["redirectUrl"];
            //}

            AuthRequestOptions requestOptions = new AuthRequestOptions()
            {
                AssertionConsumerServiceIndex = this.Options.AssertionConsumerServiceIndex,
                AttributeConsumingServiceIndex = this.Options.AttributeConsumingServiceIndex,
                Destination = this.Options.Destination,
                SPIDLevel = (SPIDLevel)this.Options.SPIDLevel,
                SPUID = this.Options.SPUID,
                UUID = Guid.NewGuid().ToString()
            };

            AuthRequest request = new AuthRequest(requestOptions);

            X509Certificate2 signinCert = new X509Certificate2(this.Options.SPIDCertPath, this.Options.SPIDCertPassword, X509KeyStorageFlags.Exportable);


            string saml = request.GetSignedAuthRequest(signinCert, this.Options.SPIDCertPrivateKey);


            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("SAMLRequest", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(saml)));
            parameters.Add("RelayState", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(properties.RedirectUri)));


            var inputs = new StringBuilder();


            foreach (var parameter in parameters)
            {
                var name = (parameter.Key);
                var value = (parameter.Value);

                var input = string.Format(CultureInfo.InvariantCulture, InputTagFormat, name, value);
                inputs.AppendLine(input);
            }



            var content = string.Format(CultureInfo.InvariantCulture, HtmlFormFormat, this.Options.Destination, inputs);
            var buffer = Encoding.UTF8.GetBytes(content);

            Response.ContentLength = buffer.Length;
            Response.ContentType = "text/html;charset=UTF-8";

            // Emit Cache-Control=no-cache to prevent client caching.
            Response.Headers[HeaderNames.CacheControl] = "no-cache";
            Response.Headers[HeaderNames.Pragma] = "no-cache";
            Response.Headers[HeaderNames.Expires] = "-1";

            await Response.Body.WriteAsync(buffer, 0, buffer.Length);
            return;
        }


        protected override Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            throw new NotImplementedException();
        }
    }
}
