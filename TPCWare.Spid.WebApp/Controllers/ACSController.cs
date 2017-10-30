using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using TPCWare.Spid.Sdk;
using TPCWare.Spid.Sdk.IdP;
using TPCWare.Spid.WebApp.Models;

namespace TPCWare.Spid.WebApp.Controllers
{
    public class ACSController : Controller
    {
        ILog log = LogManager.GetLogger(typeof(ACSController));
        private readonly string SPID_COOKIE = ConfigurationManager.AppSettings["SPID_COOKIE"];

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection formCollection)
        {
            string idpLabel;
            string spidRequestId;

            // Try to get auth requesta data from cookie
            HttpCookie cookie = Request.Cookies[SPID_COOKIE];
            if (cookie != null)
            {
                idpLabel = cookie["IdPLabel"];
                spidRequestId = cookie["SpidRequestId"];
                log.Info($"Cookie {SPID_COOKIE} IdPLabel: {idpLabel}, SpidRequestId: {spidRequestId}");
            }
            else
            {
                log.Error("Error on ACSController [HttpPost]Index method: Impossibile recuperare l'Id della sessione.");
                ViewData["Message"] = "Impossibile recuperare i dati della sessione (cookie scaduto).";
                return View("Error");
            }

            try
            {
                IdpSaml2Response idpSaml2Response = Saml2Helper.GetSpidAuthnResponse(formCollection["SAMLResponse"].ToString());

                if (!idpSaml2Response.IsSuccessful)
                {
                    Session["AppUser"] = null;

                    log.Error($"Error on ACSController [HttpPost]Index method: La risposta dell'IdP riporta il seguente StatusCode: {idpSaml2Response.StatusCodeInnerValue} con StatusMessage: {idpSaml2Response.StatusMessage} e StatusDetail: {idpSaml2Response.StatusDetail}.");
                    ViewData["Message"] = "La richiesta di identificazione è stata rifiutata.";
                    ViewData["ErrorMessage"] = $"StatusCode: {idpSaml2Response.StatusCodeInnerValue} con StatusMessage: {idpSaml2Response.StatusMessage} e StatusDetail: {idpSaml2Response.StatusDetail}.";
                    return View("Error");
                }

                // Verifica la corrispondenza del valore di spidRequestId ricavato da cookie con quello restituito dalla risposta
                if (!Saml2Helper.ValidResponse(idpSaml2Response, spidRequestId, Request.Url.ToString()))
                {
                    Session["AppUser"] = null;

                    log.Error($"Error on ACSController [HttpPost]Index method: La risposta dell'IdP non è valida (InResponseTo != spidRequestId oppure SubjectConfirmationDataRecipient != requestPath).");
                    ViewData["Message"] = "La risposta dell'IdP non è valida perché non corrisponde alla richiesta.";
                    ViewData["ErrorMessage"] = $"RequestId: _{spidRequestId}, RequestPath: {Request.Url.ToString()}, InResponseTo: {idpSaml2Response.InResponseTo}, Recipient: {idpSaml2Response.SubjectConfirmationDataRecipient}.";
                    return View("Error");
                }

                // Save request and response data needed to eventually logout as a cookie
                cookie.Values["IdPLabel"] = idpLabel;
                cookie.Values["SpidRequestId"] = spidRequestId;
                cookie.Values["SubjectNameId"] = idpSaml2Response.SubjectNameId;
                cookie.Values["AuthnStatementSessionIndex"] = idpSaml2Response.AuthnStatementSessionIndex;
                cookie.Expires = DateTime.Now.AddMinutes(20);
                Response.Cookies.Add(cookie);

                AppUser appUser = new AppUser
                {
                    Name = SpidUserInfoHelper.Name(idpSaml2Response.SpidUserInfo),
                    Surname = SpidUserInfoHelper.FamilyName(idpSaml2Response.SpidUserInfo),
                     FiscalNumber = SpidUserInfoHelper.FiscalNumber(idpSaml2Response.SpidUserInfo),
                     Email =  SpidUserInfoHelper.Email(idpSaml2Response.SpidUserInfo)
                };

                // necessario per il checkSPID
                System.Web.HttpContext.Current.Application.Lock();

                List<AppUser> allAuthenticatedUsers = (List<AppUser>)System.Web.HttpContext.Current.Application["Users"];

                if (allAuthenticatedUsers == null)

                    allAuthenticatedUsers =  new List<AppUser>();

                allAuthenticatedUsers.Add(appUser);

                System.Web.HttpContext.Current.Application["Users"] = allAuthenticatedUsers;

                System.Web.HttpContext.Current.Application.UnLock();

                Session.Add("AppUser", appUser);

                ViewData["UserInfo"] = idpSaml2Response.SpidUserInfo;

                // Save authentication data in the cookie
                //HttpCookie requestCookie = new HttpCookie("SPID_AUTHENTICATION")
                //{
                //    Expires = DateTime.Now.AddMinutes(20),
                //    Value = "true"
                //};
                //System.Web.HttpContext.Current.Response.Cookies.Add(requestCookie);

                return View("UserData");
            }

            catch (Exception ex)
            {
                log.Error("Error on ACSController [HttpPost]Index method", ex);
                ViewData["Message"] = "Errore nella lettura della risposta ricevuta dal provider.";
                ViewData["ErrorMessage"] = ex.Message;
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Logout(FormCollection collection)
        {
            ViewBag.Message = "Logout effettuato";
 
            string dataBaseInBase64 = collection[0].ToString();

            if (String.IsNullOrEmpty(dataBaseInBase64))
            {
                log.Error("Si è verificato un errore");

                return View("Error");
            }

            byte[] data = System.Convert.FromBase64String(dataBaseInBase64);

            string base64DecodedASCII = System.Text.Encoding.UTF8.GetString(data);

            log.Debug(base64DecodedASCII);

            XmlDocument xml = new XmlDocument
            {
                PreserveWhitespace = true
            };
            xml.LoadXml(base64DecodedASCII);

            if (SigningHelper.VerifySignature(xml))
            {

                Dictionary<string, string> userInfo = new Dictionary<string, string>();

                using (StringReader sr = new StringReader(base64DecodedASCII))
                {

                    string result = sr.ReadToEnd();

                    int start = result.LastIndexOf("<saml2p:SessionIndex>");

                    int end = result.LastIndexOf("</saml2p:SessionIndex>");

                    string sessionId = result.Substring(start + 22, end - start - 22);

                    //usare Id di sessione per invalidare la sessione

                    HttpContext CurrentContext = System.Web.HttpContext.Current;
                    HttpCookie requestCookie = new HttpCookie("SPID_AUTHENTICATION")
                    {
                        Expires = DateTime.Now.AddMinutes(20),
                        Value = "false"
                    };
                    CurrentContext.Response.Cookies.Add(requestCookie);

                    Session["SpidNameId"] = null;
                    Session["AppUser"] = null;

                    return View();
                }
            }
            else
            {
                return View("Error");
            }
        }
    }
}