using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;
using System.Globalization;
using System.Text;
using Developers.Italia.SPID.SAML;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace SPID_ASPNET_CORE_2_0_NoIdentity.Controllers
{
    public class SpidController : Controller
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

        private readonly IHostingEnvironment _appEnvironment;
        private IConfiguration _configuration;
        public SpidController(IHostingEnvironment appEnvironment, IConfiguration configuration)
        {
            _appEnvironment = appEnvironment;
            _configuration = configuration;
        }


        [Route("account/login")]
        public IActionResult AccountLogin()
        {
            return View("Login");
        }

        public string GetSpidAuthRequest(SpidProviderConfiguration spidProviderConfiguration)
        {
            string result = "";
            AuthRequestOptions requestOptions = new AuthRequestOptions()
            {
                AssertionConsumerServiceIndex = 0,
                AttributeConsumingServiceIndex = 2,
                Destination = spidProviderConfiguration.IdentityProviderLoginPostUrl,
                SPIDLevel = SPIDLevel.SPIDL1,
                SPUID = spidProviderConfiguration.ServiceProviderId,
                UUID = Guid.NewGuid().ToString()
            };

            AuthRequest request = new AuthRequest(requestOptions);

            X509Certificate2 signinCert = new X509Certificate2(_appEnvironment.ContentRootPath + spidProviderConfiguration.ServiceProviderCertPath, spidProviderConfiguration.ServiceProviderCertPassword, X509KeyStorageFlags.Exportable);

            if (string.IsNullOrEmpty(spidProviderConfiguration.ServiceProviderPrivatekey))
            {
                result = request.GetSignedAuthRequest(signinCert);
            }
            else
            {
                result = request.GetSignedAuthRequest(signinCert, spidProviderConfiguration.ServiceProviderPrivatekey);
            }

            return result;
        }

        public string GetSpidLogoutRequest(SpidProviderConfiguration spidProviderConfiguration)
        {
            string result = "";

            string sessionId = HttpContext.User.FindFirst("SessionId").Value ?? "";
            string nameId = HttpContext.User.FindFirst("SubjectNameId").Value ?? "";

            LogoutRequestOptions requestOptions = new LogoutRequestOptions()
            {

                Destination = spidProviderConfiguration.IdentityProviderLogoutPostUrl,
                LogoutLevel = LogoutLevel.User,
                SPUID = spidProviderConfiguration.ServiceProviderId,
                UUID = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                SubjectNameId=nameId
            };

            LogoutRequest request = new LogoutRequest(requestOptions);

            X509Certificate2 signinCert = new X509Certificate2(_appEnvironment.ContentRootPath + spidProviderConfiguration.ServiceProviderCertPath, spidProviderConfiguration.ServiceProviderCertPassword, X509KeyStorageFlags.Exportable);

            if (string.IsNullOrEmpty(spidProviderConfiguration.ServiceProviderPrivatekey))
            {
                result = request.GetSignedLogoutRequest(signinCert);
            }
            else
            {
                result = request.GetSignedLogoutRequest(signinCert, spidProviderConfiguration.ServiceProviderPrivatekey);
            }

            return result;
        }


        public async Task<IActionResult> Login([FromQuery] string providerId)
        {
            var spidProviderConfiguration = new SpidProviderConfiguration();
            _configuration.GetSection("Spid:" + providerId).Bind(spidProviderConfiguration);

            string spidAuthRequest = GetSpidAuthRequest(spidProviderConfiguration);

            string redirectUri = Request.Headers["Referer"].ToString();// Request.QueryString["RedirectUrl"];
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("SAMLRequest", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(spidAuthRequest)));
            parameters.Add("RelayState", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(redirectUri)));


            var inputs = new StringBuilder();


            foreach (var parameter in parameters)
            {
                var name = (parameter.Key);
                var value = (parameter.Value);

                var input = string.Format(CultureInfo.InvariantCulture, InputTagFormat, name, value);
                inputs.AppendLine(input);
            }



            var content = string.Format(CultureInfo.InvariantCulture, HtmlFormFormat, spidProviderConfiguration.IdentityProviderLoginPostUrl, inputs);
            var buffer = Encoding.UTF8.GetBytes(content);

            Response.ContentLength = buffer.Length;
            Response.ContentType = "text/html;charset=UTF-8";

            // Emit Cache-Control=no-cache to prevent client caching.
            Response.Headers[HeaderNames.CacheControl] = "no-cache";
            Response.Headers[HeaderNames.Pragma] = "no-cache";
            Response.Headers[HeaderNames.Expires] = "-1";

            await Response.Body.WriteAsync(buffer, 0, buffer.Length);
            return Ok();
            //return View();
        }


        [HttpPost("/spidsaml/acs")]
        public async Task<ActionResult> ACS(IFormCollection collection)
        {
            string samlResponse = "";
            string redirect = "";
            AuthResponse resp = new AuthResponse();
            try
            {
                samlResponse = Encoding.UTF8.GetString(Convert.FromBase64String(collection["SAMLResponse"]));
                redirect = Encoding.UTF8.GetString(Convert.FromBase64String(collection["RelayState"]));

                resp.Deserialize(samlResponse);

            }
            catch (Exception ex)
            {
                //TODO LOG
            }
            if (resp.RequestStatus == AuthResponse.SamlRequestStatus.Success)
            {
                //CookieOptions options = new CookieOptions();
                //options.Expires = resp.SessionIdExpireDate;
                //Response.Cookies.Delete("SPID_COOKIE");
                //Response.Cookies.Append("SPID_COOKIE", JsonConvert.SerializeObject(resp), options);

                var claims = resp.GetClaims();
                        

                var scheme = "SPIDCookie"; //CookieAuthenticationDefaults.AuthenticationScheme


                var identity = new ClaimsIdentity(claims, scheme);
                var principal = new ClaimsPrincipal(identity);

                HttpContext.User = principal;

                await AuthenticationHttpContextExtensions.SignInAsync(HttpContext, scheme, principal,
                       new AuthenticationProperties
                       {
                           ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                           IsPersistent = false,
                           AllowRefresh = false
                       });


              
            }

          if (string.IsNullOrEmpty(redirect)) { redirect = "/"; }

            return Redirect(redirect);
        }

        public async Task<IActionResult> Logout(string providerId)
        {
            var scheme = "SPIDCookie";
            await AuthenticationHttpContextExtensions.SignOutAsync(HttpContext, scheme);

            providerId = "posteid";
            var spidProviderConfiguration = new SpidProviderConfiguration();
            _configuration.GetSection("Spid:" + providerId).Bind(spidProviderConfiguration);


            string spidLogoutRequest = GetSpidLogoutRequest(spidProviderConfiguration);

            string redirectUri = Request.Headers["Referer"].ToString();// Request.QueryString["RedirectUrl"];
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("SAMLRequest", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(spidLogoutRequest)));
            parameters.Add("RelayState", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(redirectUri)));


            var inputs = new StringBuilder();


            foreach (var parameter in parameters)
            {
                var name = (parameter.Key);
                var value = (parameter.Value);

                var input = string.Format(CultureInfo.InvariantCulture, InputTagFormat, name, value);
                inputs.AppendLine(input);
            }



            var content = string.Format(CultureInfo.InvariantCulture, HtmlFormFormat, spidProviderConfiguration.IdentityProviderLogoutPostUrl, inputs);
            var buffer = Encoding.UTF8.GetBytes(content);

            Response.ContentLength = buffer.Length;
            Response.ContentType = "text/html;charset=UTF-8";

            // Emit Cache-Control=no-cache to prevent client caching.
            Response.Headers[HeaderNames.CacheControl] = "no-cache";
            Response.Headers[HeaderNames.Pragma] = "no-cache";
            Response.Headers[HeaderNames.Expires] = "-1";

            await Response.Body.WriteAsync(buffer, 0, buffer.Length);
            return Ok();

            //return RedirectToAction("Index","Home");
        }

        [HttpPost("/spidsaml/logout")]
        public async Task<ActionResult> LogoutIdp(IFormCollection collection)
        {
            string samlResponse = "";

            LogoutResponse resp = new LogoutResponse();
            try
            {
                samlResponse = Encoding.UTF8.GetString(Convert.FromBase64String(collection["SAMLResponse"]));
           
                resp.Deserialize(samlResponse);

            }
            catch (Exception ex)
            {
                //TODO LOG
            }

            //if (resp.RequestStatus == AuthResponse.SamlRequestStatus.Success)
            //{
            //   //OK
            //};

             
          
            return Redirect("/");
        }
    }



}