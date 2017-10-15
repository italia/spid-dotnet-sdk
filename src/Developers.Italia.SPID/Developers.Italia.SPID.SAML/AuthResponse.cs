using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Developers.Italia.SPID.SAML
{
    public class AuthResponse
    {
        public string Version { get; set; }

        public string Issuer { get; set; }

        public string UUID { get; set; }

        public string SPUID { get; set; }

        public string SessionId { get; set; }

        public SpidUserData User { get; set; }

        public int Status { get; set; }

        public int ErrorNumber { get; set; }

        public string ErrorDescription { get; set; }



        public  void Deserialize(string samlResponse)
        {
            ResponseType result = new ResponseType();

            XmlSerializer ser = new XmlSerializer(typeof(ResponseType));

            using (XmlReader reader = XmlReader.Create(samlResponse))
            {
                result = (ResponseType)ser.Deserialize(reader);
                this.Version= result.Version;
                this.UUID = result.ID;
                this.SPUID = result.InResponseTo;
                this.Issuer = result.Issuer.Value;

            
            }



        }


        public enum SamlRequestStatus
        {
            /// <summary>
            /// The request succeeded.
            /// </summary>
            Success,

            /// <summary>
            /// The request could not be performed due to an error on the part of the requester.
            /// </summary>
            RequesterError,

            /// <summary>
            /// The request could not be performed due to an error on the part of the SAML responder or SAML authority.
            /// </summary>
            ResponderError,

            /// <summary>
            /// The SAML responder could not process the request because the version of the request message was incorrect.
            /// </summary>
            VersionMismatchError

        }
    }
}
