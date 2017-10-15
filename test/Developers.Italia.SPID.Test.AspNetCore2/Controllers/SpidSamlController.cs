using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Developers.Italia.SPID.Test.AspNetCore2.Controllers
{
    public class SpidSamlController : Controller
    {
        // GET: SpidSaml
        public ActionResult ACS()
        {
            return View();
        }


        // POST: SpidSaml/Create
        [HttpPost]

        public ActionResult ACS(IFormCollection collection)
        {
            string mydata = "";
            string redirect = "";
            try
            {
                mydata = Encoding.UTF8.GetString(Convert.FromBase64String(collection["SAMLResponse"]));
                redirect = collection["RelayState"];

                SAML.AuthResponse resp = new SAML.AuthResponse();
                resp.Deserialize(mydata);


            }
            catch
            {

            }

            ViewData["SAMLResponse"] = mydata;
            ViewData["RelayState"] = redirect;
            return View();
        }


    }
}