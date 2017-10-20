using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using log4net;
using TPCWare.Spid.Sdk.Schema;

namespace TPCWare.Spid.Sdk
{
    public static class SigningHelper {

        public enum SignatureType {
            Response,
            Assertion,
            Request
        };

        static ILog log = log4net.LogManager.GetLogger(typeof(SigningHelper));

        /// <summary>
        /// Signs an XML Document for a Saml Response
        /// </summary>
        public static XmlElement SignDoc(XmlDocument doc, X509Certificate2 cert2, string referenceUri)
        {
            if (doc == null)
            {
                log.Error("Error on SignDoc: The doc parameter is null");
                throw new ArgumentNullException("The doc parameter can't be null");
            }

            if (cert2 == null)
            {
                log.Error("Error on SignDoc: The cert2 parameter is null");
                throw new ArgumentNullException("The cert2 parameter can't be null");
            }

            if (string.IsNullOrWhiteSpace(referenceUri))
            {
                log.Error("Error on SignDoc: The referenceUri parameter is null or empty");
                throw new ArgumentNullException("The referenceUri parameter can't be null or empty");
            }

            AsymmetricAlgorithm privateKey;

            try
            {
                privateKey = cert2.PrivateKey;
            }
            catch (Exception ex)
            {
                log.Error("Error on SignDoc: Unable to find private key in the X509Certificate");
                throw new FieldAccessException("Unable to find private key in the X509Certificate", ex);
            }

            var exportedKeyMaterial = cert2.PrivateKey.ToXmlString(true);

            var key = new RSACryptoServiceProvider(new CspParameters(24))
            {
                PersistKeyInCsp = false
            };

            key.FromXmlString(privateKey.ToXmlString(true));


            SignedXml signedXml = new SignedXml(doc)
            {
                SigningKey = key
            };
            signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

            Reference reference = new Reference
            {
                DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256"
            };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());
            reference.Uri = "#" + referenceUri;
            signedXml.AddReference(reference);

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert2));
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            XmlElement signature = signedXml.GetXml();

            return signature;
        }

        public static bool VerifyStatus(XmlDocument responseDocument)
        {
            if (responseDocument == null)
            {
                log.Error("Error on VerifySignature: the signedDocument parameter is null");
                throw new ArgumentNullException("The signedDocument parameter can't be null");
            }

            try
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(responseDocument.NameTable);
                nsmgr.AddNamespace("st", "urn:oasis:names:tc:SAML:2.0:protocol");
                string strValue = responseDocument.SelectSingleNode("//st:StatusCode", nsmgr).Attributes["Value"].Value.ToLower();
                return (strValue == "urn:oasis:names:tc:saml:2.0:status:success");
            }
            catch (Exception ex)
            {
                log.Error("Error on VerifyStatus: Unable to verify SAML response status", ex);
                throw new FieldAccessException("Unable to verify SAML response status", ex);
            }
        }

        public static bool VerifySignature(XmlDocument signedDocument)
        {
            {
                if (signedDocument == null)
                {
                    log.Error("Error on VerifySignature: the signedDocument parameter is null");
                    throw new ArgumentNullException("The signedDocument parameter can't be null");
                }

                try
                {
                    SignedXml signedXml = new SignedXml(signedDocument);

                    XmlNodeList nodeList = (signedDocument.GetElementsByTagName("ds:Signature")?.Count > 0) ?
                                           signedDocument.GetElementsByTagName("ds:Signature") :
                                           signedDocument.GetElementsByTagName("Signature");

                    signedXml.LoadXml((XmlElement)nodeList[0]);
                    return signedXml.CheckSignature();
                }
                catch (Exception ex)
                {
                    log.Error("Si è verificato un Errore durante la verifica della Signature", ex);
                    throw new Exception("Error on VerifySignature", ex);
                }
            }
        }

        public static bool AttributeSetting(out Dictionary<string, string> userInfo, out string codFiscaleIva, SPIDMetadata model)
        {
            bool res = false;
            userInfo = new Dictionary<string, string>();
            codFiscaleIva = string.Empty;

            try
            {
                userInfo.Add("Esito", "true");
                if (!String.IsNullOrEmpty(model.SpidCode))
                    userInfo.Add("spidCode", model.SpidCode);
                if (!String.IsNullOrEmpty(model.Name))
                    userInfo.Add("name", model.Name);
                if (!String.IsNullOrEmpty(model.FamilyName))
                    userInfo.Add("familyName", model.FamilyName);
                if (!String.IsNullOrEmpty(model.PlaceOfBirth))
                    userInfo.Add("placeOfBirth", model.PlaceOfBirth);
                if (!String.IsNullOrEmpty(model.CountryOfBirth))
                    userInfo.Add("countryOfBirth", model.CountryOfBirth);
                if (!String.IsNullOrEmpty(model.DateOfBirth))
                    userInfo.Add("dateOfBirth", model.DateOfBirth);
                if (!String.IsNullOrEmpty(model.Gender))
                    userInfo.Add("gender", model.Gender);
                if (!String.IsNullOrEmpty(model.CompanyName))
                    userInfo.Add("companyName", model.CompanyName);
                if (!String.IsNullOrEmpty(model.RegisteredOffice))
                    userInfo.Add("registeredOffice", model.RegisteredOffice);
                if (!String.IsNullOrEmpty(model.FiscalNumber))
                {
                    codFiscaleIva = model.FiscalNumber.Split('-')[1];
                    userInfo.Add("fiscalNumber", codFiscaleIva);
                }
                if (!String.IsNullOrEmpty(model.IvaCode))
                {
                    codFiscaleIva = model.IvaCode.Split('-')[1];
                    userInfo.Add("ivaCode", codFiscaleIva);
                }
                if (!String.IsNullOrEmpty(model.IdCard))
                    userInfo.Add("idCard", model.IdCard);
                if (!String.IsNullOrEmpty(model.MobilePhone))
                    userInfo.Add("mobilePhone", model.MobilePhone);
                if (!String.IsNullOrEmpty(model.Email))
                    userInfo.Add("email", model.Email);
                if (!String.IsNullOrEmpty(model.Address))
                    userInfo.Add("address", model.Address);
                if (!String.IsNullOrEmpty(model.ExpirationDate))
                    userInfo.Add("expirationDate", model.ExpirationDate);
                if (!String.IsNullOrEmpty(model.DigitalAddress))
                    userInfo.Add("digitalAddress", model.DigitalAddress);

                if (!String.IsNullOrEmpty(codFiscaleIva))
                {
                    using (MD5 md5Hash = MD5.Create())
                    {
                        string hash = GetMd5Hash(md5Hash, codFiscaleIva);
                        userInfo.Add("Token", hash);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error on AttributeSetting", ex);
                throw new Exception("Error on AttributeSetting", ex);
            }
            return res;

        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }

    /// <summary>Declare the signature type for rsa-sha256</summary>
    public class RsaPkCs1Sha256SignatureDescription : SignatureDescription
    {
        public RsaPkCs1Sha256SignatureDescription()
        {
            KeyAlgorithm = typeof(RSACryptoServiceProvider).FullName;
            DigestAlgorithm = typeof(SHA256CryptoServiceProvider).FullName;
            FormatterAlgorithm = typeof(RSAPKCS1SignatureFormatter).FullName;
            DeformatterAlgorithm = typeof(RSAPKCS1SignatureDeformatter).FullName;
        }

        public override AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key)
        {
            var sigProcessor =
                (AsymmetricSignatureDeformatter)CryptoConfig.CreateFromName(DeformatterAlgorithm);
            sigProcessor.SetKey(key);
            sigProcessor.SetHashAlgorithm("SHA256");
            return sigProcessor;
        }

        public override AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
        {
            var sigProcessor =
                (AsymmetricSignatureFormatter)CryptoConfig.CreateFromName(FormatterAlgorithm);
            sigProcessor.SetKey(key);
            sigProcessor.SetHashAlgorithm("SHA256");
            return sigProcessor;
        }

    }
}
