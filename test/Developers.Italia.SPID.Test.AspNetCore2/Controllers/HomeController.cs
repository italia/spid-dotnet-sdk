using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Developers.Italia.SPID.Test.AspNetCore2.Models;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using System.Xml.Serialization;
using System.IO;

namespace Developers.Italia.SPID.Test.AspNetCore2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _appEnvironment;

        public HomeController(IHostingEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        // GET: Home/Spid/5
        public ActionResult Spid(int id)
        {
            //TEST PURPOSE ONLY
            //DO NOT USE IN PRODUCTION
            //LOAD Identity Provider Info
            string configFile = string.Format("{0}\\IdentityProvider_{1}.xml", _appEnvironment.ContentRootPath,id);
            if (!System.IO.File.Exists(configFile))
            {
                return NotFound();
            }
            else
            {
                IdentityProvider idp;
                XmlSerializer xmlSerializer =new XmlSerializer(typeof(IdentityProvider));
                FileStream xmlData = new FileStream(configFile, FileMode.Open);
                idp = (IdentityProvider)xmlSerializer.Deserialize(xmlData);

                //TEST PURPOSE ONLY




                var xmlPrivateKey = idp.ServiceProviderPrivatekey;
                
                string destinationUrl = idp.IdentityProviderLoginPostUrl;
                string serviceProviderId = idp.ServiceProviderId;

                string returnUrl = "/";

                if (!string.IsNullOrEmpty(HttpContext.Request.Query["redirectUrl"]))
                {
                    returnUrl= HttpContext.Request.Query["redirectUrl"];
                }

                SAML.AuthRequestOptions requestOptions = new SAML.AuthRequestOptions()
                {
                    AssertionConsumerServiceIndex = 0,
                    AttributeConsumingServiceIndex = 2,
                    Destination = destinationUrl,
                    SPIDLevel = SAML.SPIDLevel.SPIDL1,
                    SPUID = serviceProviderId,
                    UUID = Guid.NewGuid().ToString()
                };

                SAML.AuthRequest request = new SAML.AuthRequest(requestOptions);

                X509Certificate2 signinCert = new X509Certificate2(_appEnvironment.ContentRootPath + "\\Cert\\www_dotnetcode_it.pfx", "P@ssw0rd!", X509KeyStorageFlags.Exportable);


                string saml = request.GetSignedAuthRequest(signinCert, xmlPrivateKey);


                ViewData["FormUrlAction"] = destinationUrl;
                ViewData["SAMLRequest"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(saml));
                ViewData["RelayState"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(returnUrl));

                return View();

            }
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
