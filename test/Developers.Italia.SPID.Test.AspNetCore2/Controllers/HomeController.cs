using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Developers.Italia.SPID.Test.AspNetCore2.Models;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;

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
            var exportedKeyMaterial = "<RSAKeyValue><Modulus>rBPwxOB3QM+Rhz+/zp4CQj9iySohmNjfIORulrqQPDhHs4nux5OoRPNB4oeA4QWgibwvh2Avb3C2fUQz2VlQS4+3FXMKCZE9HEKTb0nKskLaNxjQGNtvI/IzZdNtbE6OPgeMor7P2NX31l9uHfY19DdE5SuI1xLlvzJPVA2M3ZPBJg7JMQP/p5Jn1T2yFDkJ1862Ez+IEywBeOk29j4xQsi+Gg/rqb1S3ZWQWZOCy9ln7/qhpMIkloNgkLP4KPgEFZi44+DgylTW5ta3jmdOlKJnHPOf0m/L+hmS9E5nkfmMspZbJFLNlIUDyiDag527/PnBkN8C70/VmYTJoTMZ89s=</Modulus><Exponent>AQAB</Exponent><P>DTHo+xNCdsfkPoaEYOZ7C9thoyqLOm2BBluc7Vdxk/HKAwcqBGGKaDM0F1XytXZVCVrJ4s6OMYKc6W5QxiRXjRxO5yNxwnsgpXJmv+e4EJ+1CkpEQVNp0kzjljn5r7psMngx74cC2dYZRzHPKhLo4vrupCnF+p7gAdnBO5/RezCJ</P><Q>DQqK6RTWScMjB7Ki5MP9fx7mkFxRQqX91k2CULK2MlVCQYCsa+A1yXexp4PbLaP1cH7YiAnI93h/Ts1vI9tqfjrvDeBes9Dvd8l/sPW4gs9ytxD+HkyWxEhch28WpuC3DIHhSZgNFYVBebLQUuj8hZlf9ivXYBRfDHga74vjakBD</Q><DP>ADdAyFqYS7kZUqyAndUnThpYjoKzpFEGO0RkXL2BbhoWY9ZKKaguy2WLBJazUaSN34lMpBkc2lJ4npjfWV5e0EAWSlGaGsRI1Gv6okj4Cc2S+IgedbMXmAkJ7/siym0SOAEfT/u1YDrQTwnRia6lgJD6NlU4l9DheSZGsuL6WjmB</DP><DQ>Als/UnKDbfymncN3j6KicY/h1X/45vQbc9e5jl9ccKfbOv7HKQ7waSEJpt06g3q25Mbm1V3/REgvqMuSI4aILZr0iytZsWA1hQ2R1yXvWyOk9NNLN6pbK8hvf8Fg9HJyYP4u+R9SbesQM69N2U1kI7fdERxG5IOJ0TT6mQJL620F</DQ><InverseQ>AcQw/ZH/F15LmwDc62Nc+t16n492wbjvoImkPjyaaZXUBukZEEMIAaMtbj+f1cAmzQxwVnyVeJzCWP2/u0UOW8vgRUOYEhUuKd1WDq83EBNoBhRK6ap2mJ66S9uX0GOff85GO0MtEjTWI/rn3a3YFNmUKFX89XBFCwL9ceOM1kDB</InverseQ><D>M1eXxQY8TqggAbyxnBJlFiMXdHIPqC09FfFSVcLAeldIfXcwOXgDAt+Zzt8jQwCMz8vIWpoGTTfSGzoYRkdxv7nXpJy4Z/Zfx2jN2Kypv9pWhY3vuRrv5EfFsiINSf1+T1+tRHmuRkJBOkMq9eGaY42CDuaYY6ONzShTpv6MAyik6pp+r9SQ9hFFK+881ajLsYkKtpxuka61hxH6JWbi8M1hcJGaEoLiPt9aoMb0SOQegag8YW2iONHkpMRU3VLvLsyiGN6NB7O1uh0K2o4jdKFNh6c9Qk8QHVEMwDB6m0uB0QW/RtaWu3OylgutPmtu55H5mKKUcaeH7gePj/rh7CE=</D></RSAKeyValue>";

            string destinationUrl = "https://spidposte.test.poste.it/jod-fs/ssoservicepost";
            string serviceProviderId = "https://www.dotnetcode.it";
            string returnUrl = "https://localhost:44355/spidsaml/acs";

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


            string saml = request.GetSignedAuthRequest(signinCert, exportedKeyMaterial);


            ViewData["FormUrlAction"] = destinationUrl;
            ViewData["SAMLRequest"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(saml));
            ViewData["RelayState"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(returnUrl));

            return View();
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
