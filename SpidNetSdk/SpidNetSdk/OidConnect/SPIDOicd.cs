using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpidNetSdk.OidConnect
{
    public class SPIDOicd : SPIDProvider
    {
        private OicdAccountSettings accountSettings;

        public SPIDOicd(OicdAccountSettings account, AppSettings app)
        {
            this.accountSettings = account;
            base.appSettings = app;
            base.proto = SPIDProtocols.SAML2;
        }

        public override string Consume(object authResponse)
        {
            throw new NotImplementedException();
        }

        public override string GetRedirect()
        {
            throw new NotImplementedException();
        }
    }
}
