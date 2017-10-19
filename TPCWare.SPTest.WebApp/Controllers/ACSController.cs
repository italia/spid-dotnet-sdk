using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using TPCWare.SPTest.SAML;
using TPCWare.SPTest.WebApp.Models;

namespace TPCWare.SPTest.WebApp.Controllers
{
    public class ACSController : Controller
    {

        ILog Log = log4net.LogManager.GetLogger(typeof(ACSController));

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {  
            try
            { 
                string dataInBase64 = collection[0].ToString();

                if (String.IsNullOrEmpty(dataInBase64))
                {
                    Log.Error("Si è verificato un errore sulla decodificazione della risposta");
                    return View("Error");
                }

                byte[] data = System.Convert.FromBase64String(dataInBase64);
                string base64DecodedASCII = System.Text.Encoding.UTF8.GetString(data);
                Log.Debug(base64DecodedASCII);

                XmlDocument xml = new XmlDocument();
                xml.PreserveWhitespace = true;
                xml.LoadXml(base64DecodedASCII);

                if (SigningHelper.VerifySignature(xml, Log))
                {
                    string backUrl = String.Empty;
                    string requestQueryString = String.Empty;
                    string appQueryString = String.Empty;
                    string finalbackUrl = String.Empty;

                    if (System.Web.HttpContext.Current.Request.Cookies[ConfigurationManager.AppSettings["SPID_COOKIE"]] != null)
                    {

                        Guid idRequestSPID  = Guid.Parse(System.Web.HttpContext.Current.Request.Cookies[ConfigurationManager.AppSettings["SPID_COOKIE"]].Value.ToString());

                        Log.Info("Cookie ID Richiesta: " + idRequestSPID);

                    }
                    else
                    {
                        Log.Warn("Attenzione Cookie non trovato");
                    }

                    Saml2SecurityToken token = null;

                    Dictionary<string, string> spidUserInfo = new Dictionary<string, string>();

                    AppUser appUser = new AppUser();

                    using (StringReader sr = new StringReader(base64DecodedASCII))
                    {
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            reader.ReadToFollowing("Assertion", "urn:oasis:names:tc:SAML:2.0:assertion");

                            // Deserialize the token so that data can be taken from it and plugged into the RSTR
                            SecurityTokenHandlerCollection coll = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();

                            var tempToken = reader.ReadSubtree();

                            token = (Saml2SecurityToken)coll.ReadToken(tempToken);

                            spidUserInfo.Add("Esito", "true");

                            foreach (var item in token.Assertion.Statements)
                            {
                                var type = item.GetType();

                                if (type.Name == "Saml2AttributeStatement")
                                {
                                    foreach (var attr in ((System.IdentityModel.Tokens.Saml2AttributeStatement)item).Attributes)
                                    {
                                        if (attr.Name.ToLower() == "fiscalnumber" && !String.IsNullOrEmpty(attr.Values.First()))
                                        {

                                            spidUserInfo.Add(attr.Name, attr.Values.First().Split('-')[1]);
                                        }
                                        if (attr.Name.ToLower() == "ivaCode" && !String.IsNullOrEmpty(attr.Values.First()))
                                        {
                                            spidUserInfo.Add(attr.Name, attr.Values.First().Split('-')[1]);
                                        }

                                        if (attr.Name.ToLower() != "fiscalnumber" && attr.Name.ToLower() != "ivaCode" && !String.IsNullOrEmpty(attr.Values.First()))
                                            spidUserInfo.Add(attr.Name, attr.Values.First());

                                        if (attr.Name.ToLower() == "name" && !String.IsNullOrEmpty(attr.Values.First()))
                                            appUser.Name = attr.Values.First();

                                        if (attr.Name.ToLower() == "familyname" && !String.IsNullOrEmpty(attr.Values.First()))
                                            appUser.Surname = attr.Values.First();
                                    }

                                }
                            }


               
                        }

                        Session.Add("AppUser", appUser);

                        ViewData["UserInfo"] = spidUserInfo;

                        HttpCookie requestCookie = new HttpCookie("SPID_AUTHENTICATION");

                        requestCookie.Expires = DateTime.Now.AddMinutes(20);

                        requestCookie.Value = "true";

                        System.Web.HttpContext.Current.Response.Cookies.Add(requestCookie);

                        return View("UserData");
                    }

                  
                }
                else
                {
                     
                    return View("Error");
                }
            }

            catch (Exception ex)
            {
                Log.Error("Si è verificato un Errore durante l'elaborazione della risposta", ex);
               
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
                Log.Error("Si è verificato un errore");

                return View("Error");
            }

            byte[] data = System.Convert.FromBase64String(dataBaseInBase64);

            string base64DecodedASCII = System.Text.Encoding.UTF8.GetString(data);

            Log.Debug(base64DecodedASCII);

            XmlDocument xml = new XmlDocument();
            xml.PreserveWhitespace = true;
            xml.LoadXml(base64DecodedASCII);

            if (SigningHelper.VerifySignature(xml, Log))
            {
                 
                Dictionary<string, string> userInfo = new Dictionary<string, string>();

                using (StringReader sr = new StringReader(base64DecodedASCII))
                {

                    string result = sr.ReadToEnd();

                    int start = result.LastIndexOf("<saml2p:SessionIndex>");

                    int end = result.LastIndexOf("</saml2p:SessionIndex>");

                    string sessionId =result.Substring(start+22, end - start-22);

                   //usare Id di sessione per invalidare la sessione

                    HttpContext CurrentContext = System.Web.HttpContext.Current;
                    HttpCookie requestCookie = new HttpCookie("SPID_AUTHENTICATION");
                    requestCookie.Expires = DateTime.Now.AddMinutes(20);
                    requestCookie.Value = "false";
                    CurrentContext.Response.Cookies.Add(requestCookie);

                    Session["AppUser"] = null;

                    return View();
                }
            }
            else

                return View("Error");

        }
    }
}