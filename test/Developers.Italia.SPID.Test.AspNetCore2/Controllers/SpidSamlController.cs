using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;

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

        public ActionResult ACS(IFormCollection collection)
        {
            string samlResponse = "";
            string redirect = "";
            SAML.AuthResponse resp = new SAML.AuthResponse();
            try
            {
                samlResponse = Encoding.UTF8.GetString(Convert.FromBase64String(collection["SAMLResponse"]));
                redirect = collection["RelayState"];
                
                resp.Deserialize(samlResponse);
                
            }
            catch 
            {

            }

            ViewData["SAMLResponse"] = samlResponse;
            ViewData["RelayState"] = redirect;
            return View();
        }


    }
}