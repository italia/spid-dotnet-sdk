using Italia.Spid.AspNetCore.WebApp.Extensions;
using Italia.Spid.AspNetCore.WebApp.Models;
using Italia.Spid.Authentication;
using Italia.Spid.Authentication.IdP;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace Italia.Spid.AspNetCore.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;
        private IHostingEnvironment _env;

        public HomeController(IConfiguration configuration, IHostingEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public ActionResult Index()
        {
            UserInfo userInfo = HttpContext.Session.GetObject<UserInfo>("UserInfo");
            if (userInfo != null)
            {
                ViewBag.Name = userInfo.Name;
                ViewBag.Surname = userInfo.Surname;
                ViewBag.Logged = true;
            }
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Nicolò Carandini n.carandini@outlook.com , Antimo Musone antimo.musone@hotmail.com ";
            return View();
        }

        public ActionResult SpidRequest(string idpName)
        {
            // Clear user info
            HttpContext.Session.SetObject<UserInfo>("UserInfo", null);

            try
            {
                // Create the SPID request id
                string spidAuthnRequestId = Guid.NewGuid().ToString();

                // Select the Identity Provider
                IdentityProvider idp = IdentityProvidersList.GetIdpFromIdPName(idpName);

                // Retrieve the signing certificate
                var certificate = X509Helper.GetCertificateFromStore(
                    StoreLocation.LocalMachine, StoreName.My,
                    X509FindType.FindBySubjectName,
                    _configuration["Spid:CertificateName"],
                    validOnly: false);

                // Create the signed SAML request
                var spidAuthnRequest = SpidHelper.BuildSpidAuthnPostRequest(
                    uuid: spidAuthnRequestId,
                    destination: idp.SpidServiceUrl,
                    consumerServiceURL: _configuration["Spid:DomainValue"],
                    securityLevel: 1,
                    certificate: certificate,
                    identityProvider: idp,
                    enviroment: _env.EnvironmentName == "Development" ? 1 : 0);

                ViewData["data"] = spidAuthnRequest;
                ViewData["action"] = idp.SpidServiceUrl;

                //// Save the IdP label and SPID request id as a cookie
                //HttpCookie cookie = Request.Cookies.Get(SPID_COOKIE) ?? new HttpCookie(SPID_COOKIE);
                //cookie.Values["IdPName"] = idpName;
                //cookie.Values["SpidAuthnRequestId"] = spidAuthnRequestId;
                //cookie.Expires = DateTime.Now.AddMinutes(20);
                //Response.Cookies.Add(cookie);

                // Save the IdPName and SPID request id
                this.SetCookie("IdPName", idpName, 20);
                this.SetCookie("SpidAuthnRequestId", spidAuthnRequestId, 20);

                // Send the request to the Identity Provider
                return View("PostData");
            }
            catch (Exception ex)
            {
                // TODO: log.Error("Error on HomeController SpidRequest", ex);
                ViewData["Message"] = "Errore nella preparazione della richiesta di autenticazione da inviare al provider.";
                ViewData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        public ActionResult LogoutRequest()
        {
            // Try to get Authentication data from session
            string idpName = this.GetCookie("IdPName");
            string subjectNameId = this.GetCookie("SubjectNameId");
            string authnStatementSessionIndex = this.GetCookie("AuthnStatementSessionIndex");

            // End the session
            HttpContext.Session.SetObject<UserInfo>("UserInfo", null);
            this.RemoveCookie("IdPName");
            this.RemoveCookie("SpidAuthnRequestId");
            this.RemoveCookie("SpidLogoutRequestId");
            this.RemoveCookie("SubjectNameId");
            this.RemoveCookie("AuthnStatementSessionIndex");

            if (string.IsNullOrWhiteSpace(idpName) ||
                string.IsNullOrWhiteSpace(subjectNameId) ||
                string.IsNullOrWhiteSpace(authnStatementSessionIndex))
            {
                // TODO: log.Error("Error on HomeController LogoutRequest method: Impossibile recuperare i dati della sessione (sessione scaduta)");
                ViewData["Message"] = "Impossibile recuperare i dati della sessione (sessione scaduta).";
                return View("Error");
            }

            try
            {
                // Create the SPID request id and save it as a cookie
                string logoutRequestId = Guid.NewGuid().ToString();

                // Select the Identity Provider
                IdentityProvider idp = IdentityProvidersList.GetIdpFromIdPName(idpName);

                // Retrieve the signing certificate
                var certificate = X509Helper.GetCertificateFromStore(
                    StoreLocation.LocalMachine, StoreName.My,
                    X509FindType.FindBySubjectName,
                    _configuration["Spid:CertificateName"],
                    validOnly: false);

                // Create the signed SAML logout request
                var spidLogoutRequest = SpidHelper.BuildSpidLogoutPostRequest(
                    uuid: logoutRequestId,
                    consumerServiceURL: _configuration["Spid:DomainValue"],
                    certificate: certificate,
                    identityProvider: idp,
                    subjectNameId: subjectNameId,
                    authnStatementSessionIndex: authnStatementSessionIndex);

                ViewData["data"] = spidLogoutRequest;
                ViewData["action"] = idp.LogoutServiceUrl;

                // Save the IdP label and SPID logout request id
                this.SetCookie("IdPName", idpName, 20);
                this.SetCookie("SpidLogoutRequestId", logoutRequestId, 20);

                // Send the request to the Identity Provider
                return View("PostData");
            }
            catch (Exception ex)
            {
                // TODO: log.Error("Error on HomeController SpidRequest", ex);
                ViewData["Message"] = "Errore nella preparazione della richiesta di logout da inviare al provider.";
                ViewData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}