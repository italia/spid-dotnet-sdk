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

namespace Developers.Italia.SPID.Test.AspNetCore2
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
