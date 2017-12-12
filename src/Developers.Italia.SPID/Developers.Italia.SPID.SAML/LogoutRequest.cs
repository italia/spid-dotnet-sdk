using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace Developers.Italia.SPID.SAML
{

    public enum LogoutLevel
    {

        Admin = 1,

        User = 2
    }

    /// <summary>
    /// Auth Request Options
    /// </summary>
    public class LogoutRequestOptions
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthRequestOptions"/> class.
        /// </summary>
        public LogoutRequestOptions()
        {
            //SAML Protocol Default Version
            this.Version = "2.0";

            //Default SPID Level
            this.LogoutLevel = LogoutLevel.User;


            this.NotOnOrAfter = new TimeSpan(0, 5, 0);

        }


        /// <summary>
        /// Gets or sets the UUID.
        /// </summary>
        /// <value>
        /// The UUID.
        /// </value>
        public string UUID { get; set; }

        /// <summary>
        /// Gets or sets the Service Provider Unique ID.
        /// </summary>
        /// <value>
        /// The spuid.
        /// </value>
        public string SPUID { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        /// <value>
        /// The destination.
        /// </value>
        public string Destination { get; set; }

        /// <summary>
        /// Gets or sets the spid level.
        /// </summary>
        /// <value>
        /// The spid level.
        /// </value>
        public LogoutLevel LogoutLevel { get; set; }


        /// <summary>
        /// Gets or sets the not on or after.
        /// </summary>
        /// <value>
        /// The not on or after.
        /// </value>
        public TimeSpan NotOnOrAfter { get; set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the subject name identifier.
        /// </summary>
        /// <value>
        /// The subject name identifier.
        /// </value>
        public string SubjectNameId { get; set; }

    }

    /// <summary>
    /// Logout Request
    /// </summary>
    public class LogoutRequest
    {
        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public LogoutRequestOptions Options { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutRequest" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public LogoutRequest(LogoutRequestOptions options)
        {
            this.Options = options;
        }

        /// <summary>
        /// Gets the logout request.
        /// </summary>
        /// <returns></returns>
        public string GetLogoutRequest()
        {
            string result = "";
            DateTime requestDatTime = DateTime.UtcNow;

            //New LogoutRequestType
            LogoutRequestType request = new LogoutRequestType();
            request.Version = Options.Version;

            //Unique UUID
            request.ID = "_" + this.Options.UUID;

            //Request DateTime
            request.IssueInstant = requestDatTime;

            //SLO Destination URI
            request.Destination = this.Options.Destination;

            request.SessionIndex = new string[] { this.Options.SessionId };

            //Request Logout Level
            if (this.Options.LogoutLevel == LogoutLevel.Admin)
            {
                request.Reason = "urn:oasis:names:tc:SAML:2.0:logout:admin";
            }
            else
            {
                request.Reason = "urn:oasis:names:tc:SAML:2.0:logout:user";
            }

            //Issuer Data
            request.Issuer = new NameIDType()
            {
                Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:entity",
                Value = Options.SPUID,
                NameQualifier = Options.SPUID

            };

            request.Item = new NameIDType()
            {
                Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient",
                SPNameQualifier = Options.SPUID,
                Value = Options.SubjectNameId,
            };

            request.NotOnOrAfterSpecified = true;
            request.NotOnOrAfter = requestDatTime.Add(Options.NotOnOrAfter);


            string samlString = "";

            XmlSerializer serializer = new XmlSerializer(request.GetType());

            using (StringWriter stringWriter = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    Encoding = Encoding.UTF8
                };

                using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
                {
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add("saml2p", "urn:oasis:names:tc:SAML:2.0:protocol");
                    namespaces.Add("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");

                    serializer.Serialize(writer, request, namespaces);

                    samlString = stringWriter.ToString();
                }
            }
            result = samlString;


            return result;

        }

        /// <summary>
        /// Gets the signed authentication request.
        /// </summary>
        /// <param name="cert">The cert.</param>
        /// <returns></returns>
        public string GetSignedLogoutRequest(X509Certificate2 cert)
        {
            var xmlPrivateKey = "";

            //Full Framework Only
            //var xmlPrivateKey = cert.PrivateKey.ToXmlString(true);
            //.Net Standard Extension
            //var xmlPrivateKey = RSAKeyExtensions.ToXmlString((RSA)cert.PrivateKey, true);

#if NETFULL
           xmlPrivateKey = cert.PrivateKey.ToXmlString(true);
#endif

#if NETSTANDARD1_0
            xmlPrivateKey = RSAKeyExtensions.ToXmlString((RSA)cert.PrivateKey, true);
#endif


            return GetSignedLogoutRequest(cert, xmlPrivateKey);
        }

        /// <summary>
        /// Gets the signed authentication request.
        /// </summary>
        /// <param name="cert">The cert.</param>
        /// <param name="privateKey">The private key.</param>
        /// <returns></returns>
        public string GetSignedLogoutRequest(X509Certificate2 cert, string xmlPrivateKey)
        {

            string result = GetLogoutRequest();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            XmlElement signature = SignHelper.SignXmlDocument(doc, cert, xmlPrivateKey);

            doc.DocumentElement.InsertAfter(signature, doc.DocumentElement.ChildNodes[0]);

            result = doc.OuterXml;


            return result;
        }


        /// <summary>
        /// Signs the authentication request.
        /// </summary>
        /// <param name="logoutRequest">The authrequest.</param>
        /// <param name="cert">The cert.</param>
        /// <returns></returns>
        public string SignLogoutRequest(string logoutRequest, X509Certificate2 cert)
        {
            //Full Framework Only
            //var xmlPrivateKey = cert.PrivateKey.ToXmlString(true);
            //.Net Standard Extension
            //var xmlPrivateKey = RSAKeyExtensions.ToXmlString((RSA)cert.PrivateKey, true);
            var xmlPrivateKey = "";
            return SignLogoutRequest(logoutRequest, cert, xmlPrivateKey);
        }


        /// <summary>
        /// Signs the authentication request.
        /// </summary>
        /// <param name="logoutRequest">The authrequest.</param>
        /// <param name="cert">The cert.</param>
        /// <param name="xmlPrivateKey">The XML private key.</param>
        /// <returns></returns>
        public string SignLogoutRequest(string logoutRequest, X509Certificate2 cert, string xmlPrivateKey)
        {

            string result = logoutRequest;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            XmlElement signature = SignHelper.SignXmlDocument(doc, cert, xmlPrivateKey);

            doc.DocumentElement.InsertAfter(signature, doc.DocumentElement.ChildNodes[0]);

            string responseStr = doc.OuterXml;


            return result;
        }


       
    }
}
