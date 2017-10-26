using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace TPCWare.Spid.Sdk.IdP
{
    public static class IdentityProviderSelector
    {
        private static ILog log = log4net.LogManager.GetLogger(typeof(IdentityProviderSelector));

        public static IdentityProvider GetIdpFromUserChoice(string idpLabel, bool forTesting = false)
        {
            IdentityProvider idp;

            if (string.IsNullOrWhiteSpace(idpLabel))
            {
                log.Error("Error on GetIdpFromUserChoice: The idpLabel parameter is null.");
                throw new ArgumentNullException("The idpLabel parameter can't be null.");
            }

            switch (idpLabel)
            {
                case "poste_id":
                    idp = new IdentityProvider(
                        providerName: "Poste Italiane",
                        spidServiceUrl: forTesting ? "https://spidposte.test.poste.it/jod-fs/ssoservicepost" : "",
                        singleLogoutServiceUrl: forTesting ? "https://spidposte.test.poste.it/jod-fs/sloserviceresponsepost" : "",
                        now: (now) => { return now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"); },
                        after: (after) => { return after.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"); },
                        notBefore: (now) => { return now.AddMinutes(-2).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"); }
                    );
                    break;

                case "sielte_id":
                    idp = new IdentityProvider(
                        providerName: "Sielte",
                        spidServiceUrl: forTesting ? "" : "",
                        singleLogoutServiceUrl: forTesting ? "" : "",
                        now: (now) => { return now.AddMinutes(-2).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"); },
                        after: (after) => { return after.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"); },
                        notBefore: (now) => { return now.AddMinutes(-2).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"); }
                    );
                    break;

                case "tim_id":
                    idp = new IdentityProvider(
                        providerName: "TIM",
                        spidServiceUrl: forTesting ? "" : "",
                        singleLogoutServiceUrl: forTesting ? "" : "",
                        now: (now) => { return now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"); },
                        after: (after) => { return after.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"); },
                        notBefore: (now) => { return now.AddMinutes(-2).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"); }
                    );
                    break;

                case "infocert_id":
                    idp = new IdentityProvider(
                        providerName: "Infocert",
                        spidServiceUrl: forTesting ? "" : "",
                        singleLogoutServiceUrl: forTesting ? "" : "",
                        now: (now) => { return now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"); },
                        after: (after) => { return after.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"); },
                        notBefore: (now) => { return now.AddMinutes(-2).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"); }
                    );
                    break;

                default:
                    log.Error($"Error on GetIdpFromUserChoice: Identity Provider not found for idpLabel = {idpLabel}.");
                    throw new Exception($"Error on GetIdpFromUserChoice: Identity Provider not found for idpLabel = {idpLabel}.");
            }

            if (string.IsNullOrWhiteSpace(idp.SpidServiceUrl))
            {
                log.Error($"Error on GetIdpFromUserChoice: Identity Provider {idpLabel} doesn't have a {(forTesting ? "test" : "production")} endpoint.");
                throw new Exception($"Error on GetIdpFromUserChoice: Identity Provider {idpLabel} doesn't have a {(forTesting ? "test" : "production")} endpoint.");
            }

            return idp;
        }
    }
}
