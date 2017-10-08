using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpidNetSdk
{
    public abstract class SPIDProvider
    {
        public SPIDProtocols Protocol { get { return this.proto; } }

        protected SPIDProtocols proto;
        protected AppSettings appSettings;

        public abstract string GetRedirect();

        public abstract string Consume(object authResponse);
    }
}
