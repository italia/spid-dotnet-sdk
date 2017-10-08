using System;

using System.IO;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.IO.Compression;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

/// <summary>
/// Summary description for saml
/// </summary>

namespace SpidNetSdk.Saml2
{
    public class Certificate
    {
        public X509Certificate2 cert;

        public void LoadCertificate(string certificate)
        {
            cert = new X509Certificate2();
            cert.Import(StringToByteArray(certificate));
        }

        public void LoadCertificate(byte[] certificate)
        {
            cert = new X509Certificate2();
            cert.Import(certificate);
        }

        private byte[] StringToByteArray(string st)
        {
            byte[] bytes = new byte[st.Length];
            for (int i = 0; i < st.Length; i++)
            {
                bytes[i] = (byte)st[i];
            }
            return bytes;
        }
    }

    public class Response
    {
        private XmlDocument xmlDoc;
        private SamlAccountSettings accountSettings;
        private Certificate certificate;

        public Response(SamlAccountSettings accountSettings)
        {
            this.accountSettings = accountSettings;
            certificate = new Certificate();
            certificate.LoadCertificate(accountSettings.Certificate);
        }

        public void LoadXml(string xml)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.XmlResolver = null;
            xmlDoc.LoadXml(xml);
        }

        public void LoadXmlFromBase64(string response)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            LoadXml(enc.GetString(Convert.FromBase64String(response)));
        }

        public bool IsValid()
        {
            bool status = true;

            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            XmlNodeList nodeList = xmlDoc.SelectNodes("//ds:Signature", manager);

            SignedXml signedXml = new SignedXml(xmlDoc);
            signedXml.LoadXml((XmlElement)nodeList[0]);

            status &= signedXml.CheckSignature(certificate.cert, true);

            var notBefore = NotBefore();
            status &= !notBefore.HasValue || (notBefore <= DateTime.Now);

            var notOnOrAfter = NotOnOrAfter();
            status &= !notOnOrAfter.HasValue || (notOnOrAfter > DateTime.Now);

            return status;
        }

        public DateTime? NotBefore()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            var nodes = xmlDoc.SelectNodes("/samlp:Response/saml:Assertion/saml:Conditions", manager);
            string value = null;
            if (nodes != null && nodes.Count > 0 && nodes[0] != null && nodes[0].Attributes != null && nodes[0].Attributes["NotBefore"] != null)
            {
                value = nodes[0].Attributes["NotBefore"].Value;
            }
            return value != null ? DateTime.Parse(value) : (DateTime?)null;
        }

        public DateTime? NotOnOrAfter()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            var nodes = xmlDoc.SelectNodes("/samlp:Response/saml:Assertion/saml:Conditions", manager);
            string value = null;
            if (nodes != null && nodes.Count > 0 && nodes[0] != null && nodes[0].Attributes != null && nodes[0].Attributes["NotOnOrAfter"] != null)
            {
                value = nodes[0].Attributes["NotOnOrAfter"].Value;
            }
            return value != null ? DateTime.Parse(value) : (DateTime?)null;
        }

        public string GetNameID()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            XmlNode node = xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:NameID", manager);
            return node.InnerText;
        }
    }

    public class AuthRequest
    {
        public string id;
        private string issue_instant;
        private AppSettings appSettings;
        private SamlAccountSettings accountSettings;

        public enum AuthRequestFormat
        {
            CompressedBase64 = 0,
            Base64 = 1,
            Plain = 2
        }

        public AuthRequest(AppSettings appSettings, SamlAccountSettings accountSettings)
        {
            this.appSettings = appSettings;
            this.accountSettings = accountSettings;

            id = "_" + System.Guid.NewGuid().ToString();
            issue_instant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public string GetRequest(AuthRequestFormat format)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;

                using (XmlWriter xw = XmlWriter.Create(sw, xws))
                {
                    xw.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("ID", id);
                    xw.WriteAttributeString("Version", "2.0");
                    xw.WriteAttributeString("IssueInstant", issue_instant);
                    xw.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-REDIRECT");
                    xw.WriteAttributeString("AssertionConsumerServiceURL", appSettings.SamlAssertionConsumerServiceUrl);
                    //xw.WriteAttributeString("saml","xmlns", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteAttributeString("Destination", accountSettings.IdpSsoBaseUrl);
                    xw.WriteAttributeString("AttributeConsumingServiceIndex", "1");

                    //xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteAttributeString("NameQualifier", appSettings.Issuer);
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:entity");
                    xw.WriteString(appSettings.Issuer);
                    xw.WriteEndElement();

                    xw.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:transient");
                    //xw.WriteAttributeString("AllowCreate", "true");
                    xw.WriteEndElement();

                    xw.WriteStartElement("samlp", "RequestedAuthnContext", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("Comparison", "exact");

                    xw.WriteStartElement("saml", "AuthnContextClassRef", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString("https://www.spid.gov.it/SpidL2");
                    xw.WriteEndElement();

                    xw.WriteEndElement(); // RequestedAuthnContext

                    xw.WriteEndElement();
                }

                string xmlText = sw.ToString();

                switch (format)
                {
                    case AuthRequestFormat.CompressedBase64:
                        return this.compressString(xmlText);
                    case AuthRequestFormat.Base64:
                        byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(xmlText);
                        return Convert.ToBase64String(toEncodeAsBytes);
                    case AuthRequestFormat.Plain:
                    default:
                        return xmlText;
                }
            }
        }

        private string compressString(string uncompressedString)
        {
            var compressedStream = new MemoryStream();
            var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString));

            using (var compressorStream = new DeflateStream(compressedStream, CompressionMode.Compress, true))
            {
                uncompressedStream.CopyTo(compressorStream);
            }

            return Convert.ToBase64String(compressedStream.ToArray());
        }

        public string GetSignature(string input, string keyPath, string password)
        {
            char[] passwordChars = password.ToCharArray();

            using (StreamReader reader = new StreamReader(File.OpenRead(  keyPath)))
            {
                Pkcs12Store store = new Pkcs12Store(reader.BaseStream, passwordChars);
                foreach (string n in store.Aliases)
                {
                    if (store.IsKeyEntry(n))
                    {
                        AsymmetricKeyEntry keyEntry = store.GetKey(n);

                        if (keyEntry.Key.IsPrivate)
                        {
                            //var kp = (AsymmetricCipherKeyPair)pr.ReadObject();

                            ISigner sig = SignerUtilities.GetSigner("SHA256withRSA");
                            sig.Init(true, keyEntry.Key);

                            byte[] data = Encoding.UTF8.GetBytes(input);
                            sig.BlockUpdate(data, 0, data.Length);
                            byte[] computedSigBytes = sig.GenerateSignature();
                            string computedSig = Convert.ToBase64String(computedSigBytes);

                            //RsaPrivateCrtKeyParameters parameters = keyEntry.Key as RsaPrivateCrtKeyParameters;
                            return computedSig;
                        }
                    }
                }
            }
            throw new Exception("Signature generation failure");
        }
    }
}


