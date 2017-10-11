using System;

namespace Developers.Italia.SPID.SAML
{


    public class AuthRequestOptions {

    }

    public class AuthRequest
    {
        public AuthRequestOptions _options { get; set; }

        public AuthRequest(AuthRequestOptions options)
        {
            this._options = options;
        }

        public string GetAuthRequest()
        {
            string result="";

            return result;

        }

        public string GetSignedAuthRequest() {

            string result="";


            return result;
        }

    }
}
