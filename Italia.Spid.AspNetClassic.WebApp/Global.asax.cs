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

            CreateConfigDataListJsonFile();
            InitializeIdentityProviderList().GetAwaiter().GetResult();            
        }

        private async Task InitializeIdentityProviderList()
        {
            // Get the IdPs metadata
            List<IdentityProviderMetaData> idpMetadataList = null;
            string idpMetadataListUrl = ConfigurationManager.AppSettings["IDP_METADATA_LIST_URL"];
            if (!string.IsNullOrWhiteSpace(idpMetadataListUrl))
            {
                idpMetadataList = await IdentityProvidersList.GetIdpMetaDataListAsync(idpMetadataListUrl);
            }

            // Get the IdPs configuration data 
            List<IdentityProviderConfigData> idpConfigDataList = null;
            using (StreamReader sr = new StreamReader(Server.MapPath("~/idpConfigDataList.json")))
            {
                idpConfigDataList = JsonConvert.DeserializeObject<List<IdentityProviderConfigData>>(sr.ReadToEnd());
            }

            // Initialize the IdP list
            IdentityProvidersList.IdentityProvidersListFactory(idpMetadataList, idpConfigDataList);
        }

        private void CreateConfigDataListJsonFile()
        {
            List<IdentityProviderConfigData> idpConfigDataList = new List<IdentityProviderConfigData>
            {
                new IdentityProviderConfigData()
                {
                    ProviderName = "Local SPID IdP (WSO2 on Docker container for testing)",
                    SpidServiceUrl ="https://localhost:9443/samlsso",
                    LogoutServiceUrl = "https://localhost:9443/samlsso",
                    SubjectNameIdRemoveText = string.Empty,
                    DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'",
                    NowDelta = 0
                },
                new IdentityProviderConfigData()
                {
                    ProviderName = "Poste Italiane (Servizio di test)",
                    SpidServiceUrl ="https://spidposte.test.poste.it/jod-fs/ssoservicepost",
                    LogoutServiceUrl = "https://spidposte.test.poste.it/jod-fs/sloservicepost",
                    SubjectNameIdRemoveText = "SPID-",
                    DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'",
                    NowDelta = 0
                },
                //new IdentityProviderConfigData()
                //{
                //    ProviderName = "TIM",
                //    SpidServiceUrl ="",
                //    LogoutServiceUrl = "",
                //    SubjectNameIdRemoveText = "",
                //    DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'",
                //    NowDelta = 0
                //},
                //new IdentityProviderConfigData()
                //{
                //    ProviderName = "Sielte",
                //    SpidServiceUrl ="",
                //    LogoutServiceUrl = "",
                //    SubjectNameIdRemoveText = "",
                //    DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'",
                //    NowDelta = -2
                //},
                //new IdentityProviderConfigData()
                //{
                //    ProviderName = "Infocert",
                //    SpidServiceUrl ="",
                //    LogoutServiceUrl = "",
                //    SubjectNameIdRemoveText = "",
                //    DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'",
                //    NowDelta = 0
                //},
            };

            using (StreamWriter sw = new StreamWriter(Server.MapPath("~/idpConfigDataList.json")))
            {
                sw.Write(JsonConvert.SerializeObject(idpConfigDataList));
                sw.Close();
            }
        }
    }
}
