using Italia.Spid.Authentication.IdP;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Italia.Spid.AspNetCore.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;

            InitializeIdentityProviderList().GetAwaiter().GetResult();
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

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

            string identityProviderListUrl = Configuration["Spid:IdentityProviderListUrl"];
            string identityProviderListFile = Configuration["Spid:IdentityProviderListFile"];

            if (!string.IsNullOrWhiteSpace(identityProviderListUrl))
            {
              await IdentityProvidersList.LoadFromUrlAsync(identityProviderListUrl);
            }
            else
            {
          
                await IdentityProvidersList.LoadFromFileAsync(Path.Combine(HostingEnvironment.ContentRootPath, identityProviderListFile));
            }



        }

     

    }
}
