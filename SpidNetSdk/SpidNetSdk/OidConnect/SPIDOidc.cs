using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Results;
using SpidNetSdk.OidConnect.Browsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpidNetSdk.OidConnect
{
    public class SPIDOidc : SPIDProvider
    {
        private OidcAccountSettings accountSettings;

        private OidcClient oidcClient;
        private AuthorizeState authState;
        private LoginResult result;

        public SPIDOidc(OidcAccountSettings account, AppSettings app)
        {
            this.accountSettings = account;
            base.appSettings = app;
            Protocol = SPIDProtocols.OIDC;
            var options = new OidcClientOptions
            {
                Authority = accountSettings.OidcAuthority, // "https://demo.identityserver.io",
                ClientId = app.Issuer,
                Scope = app.OidcScope, // "openid profile api offline_access",
                RedirectUri = app.OidcCallbackUri, // WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri,

                Browser = app.OidcBrowser
            };
            this.oidcClient = new OidcClient(options);
        }

        public override string Consume(object authResponse)
        {
            this.result = this.oidcClient.ProcessResponseAsync((string)authResponse, this.authState).Result;
            return result.User.ToString();
        }

        public override string GetRedirect()
        {
            return this.accountSettings.OidcAuthority;
        }

        public UserInfoResult GetAttributes()
        {
            return this.oidcClient.GetUserInfoAsync(result.AccessToken).Result;
        }
    }
}
