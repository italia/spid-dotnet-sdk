using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using log4net;

namespace TPCWare.SPTest.SAML.Security.Saml
{
    public static class SigningHelper {
        public enum SignatureType {
            Response,
            Assertion,
            Request
        };
        /// <summary>
        /// Signs an XML Document for a Saml Response
        /// </summary>
        public static XmlElement SignDoc(XmlDocument doc, X509Certificate2 cert2, string referenceUri)
        {
            
          var exportedKeyMaterial = cert2.PrivateKey.ToXmlString(true);

            var key = new RSACryptoServiceProvider(new CspParameters(24));
            key.PersistKeyInCsp = false;
            key.FromXmlString(exportedKeyMaterial);
           // key.FromXmlString("<RSAKeyValue><Modulus>ppsY0DC6twkYWsWrCfy+tfLnat7yCdUbYQyPZvMx1ywmtaVCkprBraUl+81E7EgXpV1SToEP5mOBkNAeQtAXF4ROqiBGIhrWXOYDEyZgNfQTSsZA5beErLxUnVrAI1mNeyBci4aOnzGEuSDmPOJGY3xT2Zn1ehP5FWRP0n4SW0dmLX/YNLhp7lMvwR0YvkAL8sdflBboF7abWmbVrydaJHzbtCctb2d934WaYuU8GtwjTbkidupdhqE1EdfuoMOU2Gfej6hEpVgXXqr7Jk4i5eUtWcr6q7DMKAqgg2NELROZnF4EbZj0TlajYDMpDVuMOu3Rsc5Z2ZPBkD9Hz4Xy/Q==</Modulus><Exponent>AQAB</Exponent><P>z9L6jLTrEhzVZilYkXVBcLHKPWReHr6vqiqX9DiJzhHdngL8TR40snhthhoVTZaiDjy5xLhaNCK5lrEBORsHI1q7nOZJW8LP+uCpiKosJZXnK2+kkI+mogeQwbzTlVYiDRkpBD/ewML3Sj6JTO319HxlOgTZ7Hr/U4yUdEL/ykc=</P><Q>zToULKyfvS46Px7kERcO8RGCDqLFYBSQCVyhoKYakT2F9q9rOpaS7B2BhEJNA61jk3M7S4uZXDhHPpGU6EMo0Ao2WxytyQwMOCnproO4WAQTzQm/RT2mJB+WqWpcAeCarbg3b9Vnriv1yQzTQgXepf96niAZZDAnI7XGUwhotps=</Q><DP>DqnEI8lgDJccN2kTZq/vPhRNQKekPGcX3dnDfue+UVvRVyS+yHIpJa55i8yrVB4csQR31vlq4+LPVWKHw4+0oTn1osxcwKyuH/VaANqA4uYAuX/XDJwWFbiS7hh0lUTOgj4UNsiK3u7io8plxZfEkst0GPPerGDBQxPYJZvUkGE=</DP><DQ>PidAxObi2eCOM1+foq1hERFEWjphnF+d37f2GzkzApmnYLZvuyavCGNHPk72FA8HATj81DxLDerdaM2eU1lDmv38yEs/Now3hyrqYrfxtHZHqOkyzD2He5k1f8l+Y/Mp5ULNR0lSRSV7IpCHyo8MhymAcTM3fWg38lCy56K8U9E=</DQ><InverseQ>fa1BjKpqXR5dEx0JAeRZCcDjsSQ8oW7Dyo0W3d0XImGOU8rw/Lx0mzUzTS7G78krzauyEW5XEUzVJrg847o1u4P/03T50Ug7UjrVawZzakuWMatWIasoAW10ZfsZWkRqZn8mdKq3Jl4ufbmI15HPfRcDxJyiHy43yWck0vHGXB8=</InverseQ><D>Zigp0dZfVsY76cTUuJ4CblyP66bisIa8cAickZrDT9XhsnWv2WcNJSVjof9eqKcX4KzVQA/BKRqQorQKKhugXSoidgyuFFFyaaob7o0UZ2DOx4XC21hpAOXF2GqB7+sEZqAUPvV11EUvxbhXlLOGR5A/dekCbSV8ENLeYwosxUyII91Y8lI/RKawbKftsXbsWorUFRRX0sQ6J/HSkUgO98W1FWxm+YT4DA5HJ8b0qmEDPzKvmhJdAcgnl1UYwB73221KvbAJ/qEfAzVDU7mrwl3MqGB2aerZB4hJ+sww3SetGrjZ3l5TK4r17uEbwlVNPEIcZ3MTMSWQbyySZPANPQ==</D></RSAKeyValue>");


            SignedXml signedXml = new SignedXml(doc);
            signedXml.SigningKey = key;
            signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

            Reference reference = new Reference();
            reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());
            reference.Uri = String.Empty;
            reference.Uri = "#" + referenceUri;
            signedXml.AddReference(reference);

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert2));
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            XmlElement signature = signedXml.GetXml();

            return signature;
        }

        public static bool VerifyStatus(XmlDocument ResponseDocument, ILog Log)
        {
            bool res = false;
            try
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(ResponseDocument.NameTable);
                nsmgr.AddNamespace("st", "urn:oasis:names:tc:SAML:2.0:protocol");
                string strValue = ResponseDocument.SelectSingleNode("//st:StatusCode", nsmgr).Attributes["Value"].Value;
                if (strValue.ToString().ToLower() == "urn:oasis:names:tc:saml:2.0:status:success")
                {
                    res = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Si è verificato un Errore durante la verifica dello Stutus di risposta", ex);
            }
            return res;
        }

        public static bool VerifySignature(XmlDocument signedDocument, ILog Log)
        {
            bool res = false;
            XmlNodeList nodeList = null;

            try
            {
                SignedXml signedXml = new SignedXml(signedDocument);

                if ((signedDocument.GetElementsByTagName("ds:Signature") != null) && signedDocument.GetElementsByTagName("ds:Signature").Count > 0)
                {
                    nodeList = signedDocument.GetElementsByTagName("ds:Signature");
                }
                else
                {
                    nodeList = signedDocument.GetElementsByTagName("Signature");
                }

                signedXml.LoadXml((XmlElement)nodeList[0]);

                res = signedXml.CheckSignature();

                //if (!res)
                //{
                //    X509Certificate2 certificate = null;

                //    foreach (KeyInfoClause clause in signedXml.KeyInfo)
                //    {
                //        if (clause is KeyInfoX509Data)
                //        {
                //            if (((KeyInfoX509Data)clause).Certificates.Count > 0)
                //            {
                //                certificate = (X509Certificate2)((KeyInfoX509Data)clause).Certificates[0];
                //                Log.Info("Signature non valida, Certificato: " + certificate.Issuer);
                //            }
                //        }
                //    }
                //}

                //res = true;
            }
            catch (Exception ex)
            {
                Log.Error("Si è verificato un Errore durante la verifica della Signature", ex);
            }
            return res;
        }

        public static bool attributeSetting(out Dictionary<string, string> userInfo, out string codFiscaleIva, UserDataSPID model, ILog Log)
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
                Log.Error("Si è verificato un Errore durante la verifica della Signature", ex);
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
