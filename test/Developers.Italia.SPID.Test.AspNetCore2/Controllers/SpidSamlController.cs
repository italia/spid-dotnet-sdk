using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Developers.Italia.SPID.Test.AspNetCore2.Controllers
{
    public class SpidSamlController : Controller
    {
        // GET: SpidSaml/ACS
        public ActionResult ACS()
        {
            return View();
        }


        // POST: SpidSaml/ACS
        [HttpPost]

        public async Task<ActionResult> ACS(IFormCollection collection)
        {
            string samlResponse = "";
            string redirect = "";
            SAML.AuthResponse resp = new SAML.AuthResponse();
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
            if ( resp.RequestStatus== SAML.AuthResponse.SamlRequestStatus.Success)
            {
                CookieOptions options = new CookieOptions();
                options.Expires = resp.SessionIdExpireDate;
                Response.Cookies.Delete("SPID_COOKIE");
                Response.Cookies.Append("SPID_COOKIE",JsonConvert.SerializeObject(resp), options);

                var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, resp.User.Name??"", ClaimValueTypes.String, resp.Issuer),
                        new Claim(ClaimTypes.Surname, resp.User.FamilyName??"", ClaimValueTypes.String, resp.Issuer),
                        new Claim(ClaimTypes.Email, resp.User.Email??"", ClaimValueTypes.String, resp.Issuer),
                        new Claim("FiscalNumber", resp.User.FiscalNumber??"", ClaimValueTypes.String, resp.Issuer),
                        new Claim("SessionId", resp.SessionId??"", ClaimValueTypes.String, resp.Issuer),
                    };

                var userIdentity = new ClaimsIdentity(new GenericIdentity(resp.User.Name,"SPID"), claims);

                var userPrincipal = new ClaimsPrincipal(userIdentity);

           

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal,
                    new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                        IsPersistent = false,
                        AllowRefresh = false
                    });
            }

            ViewData["SAMLResponse"] = JsonConvert.SerializeObject(resp);
            ViewData["RelayState"] = redirect;
            return View();
        }
        // POST: SpidSaml/ACS
        [HttpPost]
        public ActionResult Logout(IFormCollection collection)
        {
            string samlResponse = "";
            string redirect = "";
            SAML.AuthResponse resp = new SAML.AuthResponse();
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

            ViewData["SAMLResponse"] = JsonConvert.SerializeObject(resp);
            ViewData["RelayState"] = redirect;
            return View("ACS");
        }
    }
}