using Italia.Spid.Authentication.IdP;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Italia.Spid.AspNet.WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            InitializeIdentityProviderList().GetAwaiter().GetResult();
        }

        private async Task InitializeIdentityProviderList()
        {

            string identityProviderListUrl = ConfigurationManager.AppSettings["IdentityProviderListUrl"];
            string identityProviderListFile = ConfigurationManager.AppSettings["IdentityProviderListFile"];

            if (!string.IsNullOrWhiteSpace(identityProviderListUrl))
            {
                await IdentityProvidersList.LoadFromUrlAsync(identityProviderListUrl);
            }
            else
            {
                await IdentityProvidersList.LoadFromFileAsync(Path.Combine(Server.MapPath("/"), identityProviderListFile));
            }



        }


    }
}
