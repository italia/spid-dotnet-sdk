/*
  Copyright (c) 2017 TEAM PER LA TRASFORMAZIONE DIGITALE

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini (see Git history for other contributors)
*/

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Italia.Spid.Authentication
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
        public static XmlElement SignDoc(XmlDocument doc, X509Certificate2 certificate, string referenceUri)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("The doc parameter can't be null");
            }

            if (certificate == null)
            {
                throw new ArgumentNullException("The cert2 parameter can't be null");
            }

            if (string.IsNullOrWhiteSpace(referenceUri))
            {
                throw new ArgumentNullException("The referenceUri parameter can't be null or empty");
            }

            AsymmetricAlgorithm privateKey;

            try
            {
                privateKey = certificate.PrivateKey;
            }
            catch (Exception ex)
            {
                throw new FieldAccessException("Unable to find private key in the X509Certificate", ex);
            }

            var exportedKeyMaterial = certificate.PrivateKey.ToXmlString(true);

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
            keyInfo.AddClause(new KeyInfoX509Data(certificate));
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            XmlElement signature = signedXml.GetXml();

            return signature;
        }

        public static bool VerifySignature(XmlDocument signedDocument)
        {
            {
                if (signedDocument == null)
                {
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
                    throw new Exception("Error on VerifySignature", ex);
                }
            }
        }

    }
}
