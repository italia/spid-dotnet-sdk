using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace SpidNetSdk.Saml2
{
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
                var xmlText = generateXML(sw);

                switch (format)
                {
                    case AuthRequestFormat.CompressedBase64:
                        return compressString(xmlText);
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

        private AsymmetricKeyEntry loadKey(string keyPath, string password)
        {
            char[] passwordChars = password.ToCharArray();

            using (StreamReader reader = new StreamReader(File.OpenRead(keyPath)))
            {
                Pkcs12Store store = new Pkcs12Store(reader.BaseStream, passwordChars);
                foreach (string k in store.Aliases)
                {
                    if (store.IsKeyEntry(k))
                    {
                        AsymmetricKeyEntry keyEntry = store.GetKey(k);
                        return keyEntry;
                    }
                }
            }

            throw new InvalidKeyException();
        }

        public string GetSignature(string input, string keyPath, string password)
        {
            var keyEntry = loadKey(keyPath, password);

            if (keyEntry.Key.IsPrivate)
            {
                ISigner sig = SignerUtilities.GetSigner("SHA256withRSA");
                sig.Init(true, keyEntry.Key);

                byte[] data = Encoding.UTF8.GetBytes(input);
                sig.BlockUpdate(data, 0, data.Length);

                byte[] computedSigBytes = sig.GenerateSignature();
                string computedSig = Convert.ToBase64String(computedSigBytes);

                return computedSig;
            }

            throw new Exception("Signature generation failure");
        }

        private string generateXML(StringWriter sw)
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
                    xw.WriteAttributeString("Destination", accountSettings.IdpSsoBaseUrl);
                    xw.WriteAttributeString("AttributeConsumingServiceIndex", "1");
                
                    xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteAttributeString("NameQualifier", appSettings.Issuer);
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:entity");
                    xw.WriteString(appSettings.Issuer);
                    xw.WriteEndElement();

                    xw.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:transient");
                    xw.WriteEndElement();

                    xw.WriteStartElement("samlp", "RequestedAuthnContext", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("Comparison", "exact");

                        xw.WriteStartElement("saml", "AuthnContextClassRef", "urn:oasis:names:tc:SAML:2.0:assertion");
                        xw.WriteString("https://www.spid.gov.it/SpidL1");
                        xw.WriteEndElement();

                    xw.WriteEndElement();

                xw.WriteEndElement();
            }

            return sw.ToString();
        }
    }
}
