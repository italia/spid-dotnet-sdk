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

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            if (System.Web.HttpContext.Current.Request.Cookies[ConfigurationManager.AppSettings["SPID_COOKIE"]] != null)
            {
                Guid idRequestSPID = Guid.Parse(System.Web.HttpContext.Current.Request.Cookies[ConfigurationManager.AppSettings["SPID_COOKIE"]].Value.ToString());
                log.Info("Cookie ID Richiesta: " + idRequestSPID);
            }
            else
            {
                log.Error("Error on ACSController [HttpPost]Index method: Impossibile recuperare l'Id della sessione.");
                ViewData["Message"] = "Impossibile recuperare l'Id della sessione.";
                return View("Error");
            }

            try
            {
                IdpSaml2Response idpSaml2Response = Saml2Helper.GetIdpSaml2Response(collection[0].ToString());

                if (!idpSaml2Response.IsSuccessful)
                {
                    log.Error($"Error on ACSController [HttpPost]Index method: La risposta dell'IdP riporta il seguente StatusCode: {idpSaml2Response.StatusCodeInnerValue} con StatusMessage: {idpSaml2Response.StatusMessage} e StatusDetail: {idpSaml2Response.StatusDetail}.");
                    ViewData["Message"] = "La richiesta di identificazione è stata rifiutata.";
                    ViewData["ErrorMessage"] = $"StatusCode: {idpSaml2Response.StatusCodeInnerValue} con StatusMessage: {idpSaml2Response.StatusMessage} e StatusDetail: {idpSaml2Response.StatusDetail}.";
                    return View("Error");
                }

                AppUser appUser = new AppUser
                {
                    Name = SpidUserInfoHelper.Name(idpSaml2Response.SpidUserInfo),
                    Surname = SpidUserInfoHelper.FamilyName(idpSaml2Response.SpidUserInfo)
                };

                Session.Add("AppUser", appUser);
                ViewData["UserInfo"] = idpSaml2Response.SpidUserInfo;

                // TODO: verificare la corrispondenza del valore del sessionId ricavato da cookie
                //       con quello contenuto nella proprietà idpSaml2Response.InResponseTo

                HttpCookie requestCookie = new HttpCookie("SPID_AUTHENTICATION")
                {
                    Expires = DateTime.Now.AddMinutes(20),
                    Value = "true"
                };
                System.Web.HttpContext.Current.Response.Cookies.Add(requestCookie);

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