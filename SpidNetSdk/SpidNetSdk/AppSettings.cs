using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpidNetSdk
{
    public class AppSettings
    {
        public string AssertionConsumerServiceUrl { get; set; } // "http://localhost:49573/SamlConsumer/Consume.aspx";

        public string Issuer { get; set; } // "test-app";
    }
}
