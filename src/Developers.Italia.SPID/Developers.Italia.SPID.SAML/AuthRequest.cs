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

    public enum SPIDLevel
    {

        SPIDL1 = 1,

        SPIDL2 = 2,

        SPIDL3 = 3
    }

    /// <summary>
    /// Auth Request Options
    /// </summary>
    public class AuthRequestOptions
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthRequestOptions"/> class.
        /// </summary>
        public AuthRequestOptions()
        {
            //SAML Protocol Default Version
            this.Version = "2.0";

            //Default SPID Level
            this.SPIDLevel = SPIDLevel.SPIDL1;

            this.NotBefore = new TimeSpan(0, -2, 0);

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
        public SPIDLevel SPIDLevel { get; set; }

        /// <summary>
        /// Gets or sets the index of the assertion consumer service.
        /// Refer to Service Provider Metadata Index Value On AssertionConsumerService
        /// </summary>
        /// <value>
        /// The index of the assertion consumer service.
        /// </value>
        public ushort AssertionConsumerServiceIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the attribute consuming service.
        /// </summary>
        /// <value>
        /// The index of the attribute consuming service.
        /// </value>
        public ushort AttributeConsumingServiceIndex { get; set; }

        public TimeSpan NotBefore { get; set; }
        public TimeSpan NotOnOrAfter { get; set; }



    }
    /// <summary>
    /// Authorization Request
    /// Refer to spid-regole_tecniche_v1.pdf
    /// 1.2.2.1. AUTHNREQUEST
    /// </summary>
    public class AuthRequest
    {
        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>

        public AuthRequestOptions Options { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthRequest"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public AuthRequest(AuthRequestOptions options)
        {
            this.Options = options;
        }

        /// <summary>
        /// Gets the authentication request.
        /// </summary>
        /// <returns></returns>
        public string GetAuthRequest()
        {
            string result = "";
            DateTime requestDatTime = DateTime.UtcNow;
            //New AuthnRequestType
            AuthnRequestType request = new AuthnRequestType();
            request.Version = Options.Version;

            //Unique UUID
            request.ID = "_" +this.Options.UUID;

            //Request DateTime
            request.IssueInstant = requestDatTime;

            //Request Force Authn
            if ((int)Options.SPIDLevel > 1)
            {
                request.ForceAuthn = true;
                request.ForceAuthnSpecified = true;
            }
            else
            {
                request.ForceAuthn = false;
                request.ForceAuthnSpecified = true;
            }


            //SSO Destination URI
            request.Destination = this.Options.Destination;

            //Service Provider Assertion Consumer Service Index
            request.AssertionConsumerServiceIndex = this.Options.AssertionConsumerServiceIndex;
            request.AssertionConsumerServiceIndexSpecified = true;

            //Service Provider Attribute Consumer Service Index
            request.AttributeConsumingServiceIndex = this.Options.AttributeConsumingServiceIndex;
            request.AttributeConsumingServiceIndexSpecified = true;


            //Service Provider Attribute Consumer Service Index
            request.AttributeConsumingServiceIndex = this.Options.AttributeConsumingServiceIndex;
            request.AttributeConsumingServiceIndexSpecified = true;

            //Issuer Data
            request.Issuer = new NameIDType()
            {
                Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:entity",
                Value = Options.SPUID,
                NameQualifier = Options.SPUID

            };

            request.NameIDPolicy = new NameIDPolicyType()
            {
                Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient",
                AllowCreate = true
            };

            //NotRequired
            request.Conditions = new ConditionsType()
            {
                NotBefore = requestDatTime.Add(this.Options.NotBefore),
                NotBeforeSpecified=true,
                NotOnOrAfter = requestDatTime.Add(this.Options.NotOnOrAfter),
                NotOnOrAfterSpecified=true
            };

            RequestedAuthnContextType requestedAuthn = new RequestedAuthnContextType
            {
                Comparison = AuthnContextComparisonType.minimum,
                ComparisonSpecified = true,
                ItemsElementName = new ItemsChoiceType7[] { ItemsChoiceType7.AuthnContextClassRef },
                Items = new string[] { "https://www.spid.gov.it/SpidL" + ((int)Options.SPIDLevel).ToString() }
            };

            request.RequestedAuthnContext = requestedAuthn;


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
        public string GetSignedAuthRequest(X509Certificate2 cert)
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
           

            return GetSignedAuthRequest(cert, xmlPrivateKey);
        }

        /// <summary>
        /// Gets the signed authentication request.
        /// </summary>
        /// <param name="cert">The cert.</param>
        /// <param name="privateKey">The private key.</param>
        /// <returns></returns>
        public string GetSignedAuthRequest(X509Certificate2 cert, string xmlPrivateKey)
        {

            string result = GetAuthRequest();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            XmlElement signature = SignXmlDocument(doc, cert, xmlPrivateKey);

            doc.DocumentElement.InsertAfter(signature, doc.DocumentElement.ChildNodes[0]);

            result =  doc.OuterXml;


            return result;
        }


        /// <summary>
        /// Signs the authentication request.
        /// </summary>
        /// <param name="authrequest">The authrequest.</param>
        /// <param name="cert">The cert.</param>
        /// <returns></returns>
        public string SignAuthRequest(string authrequest, X509Certificate2 cert)
        {
            //Full Framework Only
            //var xmlPrivateKey = cert.PrivateKey.ToXmlString(true);
            //.Net Standard Extension
            //var xmlPrivateKey = RSAKeyExtensions.ToXmlString((RSA)cert.PrivateKey, true);
            var xmlPrivateKey = "";
            return SignAuthRequest(authrequest, cert, xmlPrivateKey);
        }


        /// <summary>
        /// Signs the authentication request.
        /// </summary>
        /// <param name="authrequest">The authrequest.</param>
        /// <param name="cert">The cert.</param>
        /// <param name="xmlPrivateKey">The XML private key.</param>
        /// <returns></returns>
        public string SignAuthRequest(string authrequest, X509Certificate2 cert, string xmlPrivateKey)
        {

            string result = authrequest;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            XmlElement signature = SignXmlDocument(doc, cert,xmlPrivateKey);

            doc.DocumentElement.InsertAfter(signature, doc.DocumentElement.ChildNodes[0]);

            string responseStr = doc.OuterXml;


            return result;
        }


        /// <summary>
        /// Signs the XML document.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="cert">The cert.</param>
        /// <returns></returns>
        private XmlElement SignXmlDocument(XmlDocument doc, X509Certificate2 cert)
        {
            //Full Framework Only
            //var xmlPrivateKey = cert.PrivateKey.ToXmlString(true);
            //.Net Standard Extension
            var xmlPrivateKey = RSAKeyExtensions.ToXmlString((RSA)cert.PrivateKey, true);
            
            return SignXmlDocument(doc, cert, xmlPrivateKey);
        }


        /// <summary>
        /// Signs the XML document.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="cert">The cert.</param>
        /// <param name="xmlPrivateKey">The XML private key.</param>
        /// <returns></returns>
        private XmlElement SignXmlDocument(XmlDocument doc, X509Certificate2 cert, string xmlPrivateKey)
        {

        

            var key = new RSACryptoServiceProvider(new CspParameters(24));
            key.PersistKeyInCsp = false;
            //Full Framework Only
            //key.FromXmlString(xmlPrivateKey);
            //.Net Standard Extension
            RSAKeyExtensions.FromXmlString(key, xmlPrivateKey);

            SignedXml signedXml = new SignedXml(doc);
            signedXml.SigningKey = key;
            signedXml.SignedInfo.SignatureMethod = key.SignatureAlgorithm;// "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

            Reference reference = new Reference();
            reference.Uri = "";
            reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());

            signedXml.AddReference(reference);

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            XmlElement signature = signedXml.GetXml();

            return signature;
        }

    }
}
