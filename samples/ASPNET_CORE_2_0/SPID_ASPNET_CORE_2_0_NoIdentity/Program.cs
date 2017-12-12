using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace SPID_ASPNET_CORE_2_0_NoIdentity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
             .UseStartup<Startup>()
             .ConfigureAppConfiguration((builderContext, config) =>
             {
                 var env = builderContext.HostingEnvironment;

                 config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                 ;
             })
              .UseKestrel(options =>
              {
                  options.Listen(IPAddress.Loopback, 5000);
                  options.Listen(IPAddress.Loopback, 44355, listenOptions =>
                  {
                      listenOptions.UseHttps("cert/localhost.pfx", "P@ssw0rd!");
                      listenOptions.UseConnectionLogging();
                  });
              })
             .Build();

        }
    }
}
