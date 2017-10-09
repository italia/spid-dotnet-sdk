using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TPCWare.SPTest.AspNetCore.WebApp.Models;
using log4net;
using TPCWare.SPTest.SAML2;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Xml;
using System.IdentityModel.Tokens;
using System.IO;

namespace TTPCWare.SPTest.AspNetCore.WebApp.Controllers
{
    public class ACSController : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly SpidOptions _spidOptions;

        public ACSController(IOptions<SpidOptions> spidOptionsAccessor, IHttpContextAccessor contextAccessor)
        {
            _spidOptions = spidOptionsAccessor.Value;
            _contextAccessor = contextAccessor;
        }

        ILog Log = log4net.LogManager.GetLogger(typeof(ACSController));
       
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection collection)
        {
            Guid idRequest;
            String codicefiscaleIva = "";

            HttpContext CurrentContext = _contextAccessor.HttpContext;
            string spidCookieName = _spidOptions.CookieId;

            try
            {
                string dataBaseInBase64 = collection.First().ToString();

                if (String.IsNullOrEmpty(dataBaseInBase64))
                {
                    Log.Error("Si è verificato un errore");
                    return View("Error");
                }

                byte[] data = System.Convert.FromBase64String(dataBaseInBase64);
                string base64DecodedASCII = System.Text.Encoding.UTF8.GetString(data);
                Log.Debug(base64DecodedASCII);

                XmlDocument xml = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                xml.LoadXml(base64DecodedASCII);

                if (SigningHelper.VerifySignature(xml, Log))
                {
                    string backUrl = String.Empty;
                    string requestQueryString = String.Empty;
                    string appQueryString = String.Empty;
                    string finalbackUrl = String.Empty;

                    if (CurrentContext.Request.Cookies[spidCookieName] != null)
                    {

                        // TODO: use coockie
                        // idRequest = Guid.Parse(CurrentContext.Request.Cookies[spidCookieName].Value.ToString());
                        idRequest = Guid.NewGuid();

                        Log.Info("Recupero Richiesta: " + idRequest);

                    }
                    else
                    {
                        Log.Warn("Cookie non trovato, impossibile proseguire.");
                    }

                    Saml2SecurityToken token = null;

                    Dictionary<string, string> userInfo = new Dictionary<string, string>();
                    using (StringReader sr = new StringReader(base64DecodedASCII))
                    {
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            reader.ReadToFollowing("Assertion", "urn:oasis:names:tc:SAML:2.0:assertion");

                            // Deserialize the token so that data can be taken from it and plugged into the RSTR
                            SecurityTokenHandlerCollection coll = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();

                            var tempToken = reader.ReadSubtree();
                            token = (Saml2SecurityToken)coll.ReadToken(tempToken);
                            userInfo.Add("Esito", "true");

                            foreach (var item in token.Assertion.Statements)
                            {
                                var type = item.GetType();

                                if (type.Name == "Saml2AttributeStatement")
                                {
                                    foreach (var attr in ((System.IdentityModel.Tokens.Saml2AttributeStatement)item).Attributes)
                                    {
                                        if (attr.Name.ToLower() == "fiscalnumber" && !String.IsNullOrEmpty(attr.Values.First()))
                                        {
                                            codicefiscaleIva = attr.Values.First().Split('-')[1];
                                            userInfo.Add(attr.Name, attr.Values.First().Split('-')[1]);
                                        }
                                        if (attr.Name.ToLower() == "ivaCode" && !String.IsNullOrEmpty(attr.Values.First()))
                                        {
                                            codicefiscaleIva = attr.Values.First().Split('-')[1];
                                            userInfo.Add(attr.Name, attr.Values.First().Split('-')[1]);
                                        }
                                        if (attr.Name.ToLower() != "fiscalnumber" && attr.Name.ToLower() != "ivaCode" && !String.IsNullOrEmpty(attr.Values.First()))
                                            userInfo.Add(attr.Name, attr.Values.First());
                                    }

                                }
                            }
                        }
                    }

                    ViewData["UserInfo"] = userInfo;

                    return View("UserData");
                }
                else
                {

                    ViewData["Message"] = "Ci dispiace ma si è verificato un errore.";
                    return View("Error");
                }
            }

            catch (Exception ex)
            {
                Log.Error("Si è verificato un Errore durante il Processamento della risposta", ex);
               
                return View("Error");
            }
        }
    }
}