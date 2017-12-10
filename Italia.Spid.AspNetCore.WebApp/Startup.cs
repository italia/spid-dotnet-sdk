using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Italia.Spid.Authentication.IdP;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Italia.Spid.AspNetCore.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            CreateConfigDataListJsonFile();
            InitializeIdentityProviderList().GetAwaiter().GetResult();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.Name = ".Spid.Session";
                // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private async Task InitializeIdentityProviderList()
        {
            // Get the IdPs metadata
            List<IdentityProviderMetaData> idpMetadataList = null;
            string idpMetadataListUrl = Configuration["Spid:IdpMetadataListUrl"];
            if (!string.IsNullOrWhiteSpace(idpMetadataListUrl))
            {
                idpMetadataList = await IdentityProvidersList.GetIdpMetaDataListAsync(idpMetadataListUrl);
            }

            // Get the IdPs configuration data 
            List<IdentityProviderConfigData> idpConfigDataList = null;
            using (StreamReader sr = new StreamReader("idpConfigDataList.json"))
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

            using (StreamWriter sw = new StreamWriter("idpConfigDataList.json"))
            {
                sw.Write(JsonConvert.SerializeObject(idpConfigDataList));
                sw.Close();
            }
        }
    }
}
