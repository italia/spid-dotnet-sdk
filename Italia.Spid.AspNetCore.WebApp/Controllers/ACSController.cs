using System;
using System.Collections.Generic;
using Italia.Spid.AspNetCore.WebApp.Extensions;
using Italia.Spid.AspNetCore.WebApp.Models;
using Italia.Spid.Authentication;
using Italia.Spid.Authentication.IdP;
using Italia.Spid.Authentication.Schema;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Italia.Spid.AspNet.WebApp.Controllers
{
    public class ACSController : Controller
    {
        private IConfiguration _configuration;
        private IHostingEnvironment _env;

        public ACSController(IConfiguration configuration, IHostingEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(IFormCollection formCollection)
        {
            // Try to get auth requesta data
            string idPName = this.GetCookie("IdPName");
            string spidAuthnRequestId = this.GetCookie("SpidAuthnRequestId");

            if (string.IsNullOrWhiteSpace(idPName) || string.IsNullOrWhiteSpace(spidAuthnRequestId))
            {
                // TODO: log.Error("Error on ACSController [HttpPost]Index method: Impossibile recuperare l'Id della sessione.");
                ViewData["Message"] = "Impossibile recuperare i dati della sessione (cookie scaduto).";
                return View("Error");
            }

            try
            {
                IdpAuthnResponse idpAuthnResponse = SpidHelper.GetSpidAuthnResponse(formCollection["SAMLResponse"].ToString());

                if (!idpAuthnResponse.IsSuccessful)
                {
                    // End the session
                    this.RemoveCookie("IdPName");
                    this.RemoveCookie("SpidAuthnRequestId");
                    this.RemoveCookie("SpidLogoutRequestId");
                    this.RemoveCookie("SubjectNameId");
                    this.RemoveCookie("AuthnStatementSessionIndex");

                    // TODO: log.Error($"Error on ACSController [HttpPost]Index method: La risposta dell'IdP riporta il seguente StatusCode: {idpAuthnResponse.StatusCodeInnerValue} con StatusMessage: {idpAuthnResponse.StatusMessage} e StatusDetail: {idpAuthnResponse.StatusDetail}.");
                    ViewData["Message"] = "La richiesta di identificazione è stata rifiutata.";
                    ViewData["ErrorMessage"] = $"StatusCode: {idpAuthnResponse.StatusCodeInnerValue} con StatusMessage: {idpAuthnResponse.StatusMessage} e StatusDetail: {idpAuthnResponse.StatusDetail}.";
                    return View("Error");
                }

                string requestUrl = $"http{(Request.IsHttps ? "s" : "")}://{Request.Host.ToString()}{Request.Path.ToString()}";

                // Verifica la corrispondenza del valore di spidAuthnRequestId ricavato dalla sessione con quello restituito dalla risposta
                if (!SpidHelper.ValidAuthnResponse(idpAuthnResponse, spidAuthnRequestId, requestUrl))
                {
                    // TODO: log.Error($"Error on ACSController [HttpPost]Index method: La risposta dell'IdP non è valida (InResponseTo != spidAuthnRequestId oppure SubjectConfirmationDataRecipient != requestPath).");
                    ViewData["Message"] = "La risposta dell'IdP non è valida perché non corrisponde alla richiesta.";
                    ViewData["ErrorMessage"] = $"RequestId: _{spidAuthnRequestId}, RequestPath: {requestUrl}, InResponseTo: {idpAuthnResponse.InResponseTo}, Recipient: {idpAuthnResponse.SubjectConfirmationDataRecipient}.";
                    return View("Error");
                }

                HttpContext.Session.SetObject<UserInfo>("UserInfo", new UserInfo
                {
                    Name = SpidUserInfoHelper.Name(idpAuthnResponse.SpidUserInfo),
                    Surname = SpidUserInfoHelper.FamilyName(idpAuthnResponse.SpidUserInfo),
                    FiscalNumber = SpidUserInfoHelper.FiscalNumber(idpAuthnResponse.SpidUserInfo),
                    Email = SpidUserInfoHelper.Email(idpAuthnResponse.SpidUserInfo)
                });

                // Save request and response data needed to eventually logout
                this.SetCookie("IdPName", idPName, 20);
                this.SetCookie("SpidAuthnRequestId", spidAuthnRequestId, 20);
                this.SetCookie("SubjectNameId", idpAuthnResponse.SubjectNameId, 20);
                this.SetCookie("AuthnStatementSessionIndex", idpAuthnResponse.AuthnStatementSessionIndex, 20);

                ViewData["UserInfo"] = idpAuthnResponse.SpidUserInfo;
                return View("UserData");
            }

            catch (Exception ex)
            {
                // TODO: log.Error("Error on ACSController [HttpPost]Index method", ex);
                ViewData["Message"] = "Errore nella lettura della risposta ricevuta dal provider.";
                ViewData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Logout(IFormCollection formCollection)
        {
            // Try to get logout request data
            string idPName = this.GetCookie("IdPName");
            string spidLogoutRequestId = this.GetCookie("SpidLogoutRequestId");

            // End the session and remove cookies
            HttpContext.Session.SetObject<UserInfo>("UserInfo", null);
            this.RemoveCookie("IdPName");
            this.RemoveCookie("SpidAuthnRequestId");
            this.RemoveCookie("SpidLogoutRequestId");
            this.RemoveCookie("SubjectNameId");
            this.RemoveCookie("AuthnStatementSessionIndex");

            // The single logout process can be started directly by IdP provider without a client request done from this service
            // So an empty spidLogoutRequestId is still valid and do not represent an error.
            // In that context, we simply skip the response processing, since there isn't any response to check.
            if (!string.IsNullOrWhiteSpace(spidLogoutRequestId))
            {
                try
                {
                    IdpLogoutResponse idpLogoutResponse = SpidHelper.GetSpidLogoutResponse(formCollection["SAMLResponse"].ToString());

                    if (!idpLogoutResponse.IsSuccessful)
                    {
                        // TODO: log.Error($"Error on ACSController [HttpPost]Index method: La risposta dell'IdP riporta il seguente StatusCode: {idpLogoutResponse.StatusCodeInnerValue} con StatusMessage: {idpLogoutResponse.StatusMessage} e StatusDetail: {idpLogoutResponse.StatusDetail}.");
                        ViewData["Message"] = "La richiesta di logout è stata rifiutata.";
                        ViewData["ErrorMessage"] = $"StatusCode: {idpLogoutResponse.StatusCodeInnerValue} con StatusMessage: {idpLogoutResponse.StatusMessage} e StatusDetail: {idpLogoutResponse.StatusDetail}.";
                        return View("Error");
                    }

                    // Verifica la corrispondenza del valore di spidLogoutRequestId ricavato da cookie con quello restituito dalla risposta
                    if (!SpidHelper.ValidLogoutResponse(idpLogoutResponse, spidLogoutRequestId))
                    {
                        // TODO: log.Error($"Error on ACSController [HttpPost]Index method: La risposta dell'IdP non è valida (InResponseTo != spidLogoutRequestId).");
                        ViewData["Message"] = "La risposta dell'IdP non è valida perché non corrisponde alla richiesta.";
                        ViewData["ErrorMessage"] = $"RequestId: _{spidLogoutRequestId}, InResponseTo: {idpLogoutResponse.InResponseTo}.";
                        return View("Error");
                    }
                }

                catch (Exception ex)
                {
                    // TODO: log.Error("Error on ACSController [HttpPost]Index method", ex);
                    ViewData["Message"] = "Errore nella lettura della risposta ricevuta dal provider.";
                    ViewData["ErrorMessage"] = ex.Message;
                    return View("Error");
                }
            }

            return View("Logout");
        }

    }
}