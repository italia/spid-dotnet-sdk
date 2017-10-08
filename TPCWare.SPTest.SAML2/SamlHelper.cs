using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml.Serialization;
using System.IO;

namespace TPCWare.SPTest.SAML2
{
    public static class SamlHelper
    {
        /// <summary>
        /// Creates a Version 1.1 Saml Assertion
        /// </summary>
        /// <param name="issuer">Issuer</param>
        /// <param name="subject">Subject</param>
        /// <param name="attributes">Attributes</param>
        /// <returns>returns a Version 1.1 Saml Assertion</returns>
        private static AssertionType CreateSamlAssertion(string issuer, string recipient, string domain, string subject, Dictionary<string, string> attributes)
        {
            // Here we create some SAML assertion with ID and Issuer name. 
            AssertionType assertion = new AssertionType
            {
                ID = "_" + Guid.NewGuid().ToString()
            };

            NameIDType issuerForAssertion = new NameIDType
            {
                Value = issuer.Trim()
            };

            assertion.Issuer = issuerForAssertion;
            assertion.Version = "2.0";

            assertion.IssueInstant = DateTime.UtcNow;

            //Not before, not after conditions 
            ConditionsType conditions = new ConditionsType();
            DateTime now = DateTime.UtcNow;
            string nowString = now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            DateTime after = now.AddMinutes(10);
            string afterString = after.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            conditions.NotBefore = nowString;
            conditions.NotBeforeSpecified = true;
            conditions.NotOnOrAfter = afterString;
            conditions.NotOnOrAfterSpecified = true;

            AudienceRestrictionType audienceRestriction = new AudienceRestrictionType
            {
                Audience = new string[] { domain.Trim() }
            };

            conditions.Items = new ConditionAbstractType[] { audienceRestriction };

            //Name Identifier to be used in Saml Subject
            NameIDType nameIdentifier = new NameIDType
            {
                NameQualifier = domain.Trim(),
                Value = subject.Trim()
            };

            SubjectConfirmationType subjectConfirmation = new SubjectConfirmationType();
            SubjectConfirmationDataType subjectConfirmationData = new SubjectConfirmationDataType();

            subjectConfirmation.Method = "urn:oasis:names:tc:SAML:2.0:cm:bearer";
            subjectConfirmation.SubjectConfirmationData = subjectConfirmationData;
            // 
            // Create some SAML subject. 
            SubjectType samlSubject = new SubjectType();

            AttributeStatementType attrStatement = new AttributeStatementType();
            AuthnStatementType authStatement = new AuthnStatementType
            {
                AuthnInstant = DateTime.UtcNow
            };
            AuthnContextType context = new AuthnContextType
            {
                ItemsElementName = new ItemsChoiceType5[] { ItemsChoiceType5.AuthnContextClassRef },
                Items = new object[] { "AuthnContextClassRef" }
            };
            authStatement.AuthnContext = context;

            samlSubject.Items = new object[] { nameIdentifier, subjectConfirmation };

            assertion.Subject = samlSubject;

            IPHostEntry ipEntry =
                Dns.GetHostEntry(System.Environment.MachineName);

            SubjectLocalityType subjectLocality = new SubjectLocalityType
            {
                Address = ipEntry.AddressList[0].ToString()
            };

            attrStatement.Items = new AttributeType[attributes.Count];
            int i = 0;
            // Create userName SAML attributes. 
            foreach (KeyValuePair<string, string> attribute in attributes)
            {
                AttributeType attr = new AttributeType
                {
                    Name = attribute.Key,
                    NameFormat = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic",
                    AttributeValue = new object[] { attribute.Value }
                };
                attrStatement.Items[i] = attr;
                i++;
            }
            assertion.Conditions = conditions;

            assertion.Items = new StatementAbstractType[] { authStatement, attrStatement };

            return assertion;

        }
        /// <summary>
        /// GetPostSamlResponse - Returns a Base64 Encoded String with the SamlResponse in it.
        /// </summary>
        /// <param name="recipient">Recipient</param>
        /// <param name="issuer">Issuer</param>
        /// <param name="domain">Domain</param>
        /// <param name="subject">Subject</param>
        /// <param name="storeLocation">Certificate Store Location</param>
        /// <param name="storeName">Certificate Store Name</param>
        /// <param name="findType">Certificate Find Type</param>
        /// <param name="certLocation">Certificate Location</param>
        /// <param name="findValue">Certificate Find Value</param>
        /// <param name="certFile">Certificate File (used instead of the above Certificate Parameters)</param>
        /// <param name="certPassword">Certificate Password (used instead of the above Certificate Parameters)</param>
        /// <param name="attributes">A list of attributes to pass</param>
        /// <param name="signatureType">Whether to sign Response or Assertion</param>
        /// <returns>A base64Encoded string with a SAML response.</returns>
        public static string GetPostSamlResponse(string recipient, string issuer, string domain, string subject,
            StoreLocation storeLocation, StoreName storeName, X509FindType findType, string certFile, string certPassword, object findValue,
            Dictionary<string, string> attributes, SigningHelper.SignatureType signatureType)
        {
            ResponseType response = new ResponseType
            {
                // Response Main Area
                ID = "_" + Guid.NewGuid().ToString(),
                Destination = recipient,
                Version = "2.0",
                IssueInstant = DateTime.UtcNow
            };

            NameIDType issuerForResponse = new NameIDType
            {
                Value = issuer.Trim()
            };

            response.Issuer = issuerForResponse;

            StatusType status = new StatusType
            {
                StatusCode = new StatusCodeType()
            };
            status.StatusCode.Value = "urn:oasis:names:tc:SAML:2.0:status:Success";

            response.Status = status;

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("saml2p", "urn:oasis:names:tc:SAML:2.0:protocol");
            ns.Add("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");

            XmlSerializer responseSerializer =
                new XmlSerializer(response.GetType());

            StringWriter stringWriter = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                Encoding = Encoding.UTF8
            };

            XmlWriter responseWriter = XmlWriter.Create(stringWriter, settings);

            string samlString = string.Empty;

            AssertionType assertionType = SamlHelper.CreateSamlAssertion(
                issuer.Trim(), recipient.Trim(), domain.Trim(), subject.Trim(), attributes);

            response.Items = new AssertionType[] { assertionType };

            responseSerializer.Serialize(responseWriter, response, ns);
            responseWriter.Close();

            samlString = stringWriter.ToString();

            samlString = samlString.Replace("SubjectConfirmationData",
                string.Format("SubjectConfirmationData NotOnOrAfter=\"{0:o}\" Recipient=\"{1}\"",
                DateTime.UtcNow.AddMinutes(5), recipient));

            samlString = samlString.Replace("<saml2:Assertion ", "<saml2:Assertion xmlns:saml2=\"urn:oasis:names:tc:SAML:2.0:assertion\" ");

            samlString = samlString.Replace("<saml2:AuthnContextClassRef>AuthnContextClassRef</saml2:AuthnContextClassRef>", "<saml2:AuthnContextClassRef>" + issuer + "</saml2:AuthnContextClassRef>");

            stringWriter.Close();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(samlString);
            X509Certificate2 cert = null;
            if (System.IO.File.Exists(certFile))
            {
                cert = new X509Certificate2(certFile, certPassword);
            }
            else
            {
                X509Store store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection CertCol = store.Certificates;

                //foreach (X509Certificate2 c in CertCol)
                //{
                //    if (c.Subject.Contains(findValue.ToString()))
                //    {
                //        cert = c;
                //        break;
                //    }
                //}

                X509Certificate2Collection coll = store.Certificates.Find(findType, findValue.ToString(), false);

                //if (cert == null)
                //{
                //    throw new ArgumentException("Unable to locate certificate");
                //}
                if (coll.Count < 1)
                {
                    throw new ArgumentException("Unable to locate certificate");
                }
                cert = coll[0];
                store.Close();
            }

            XmlElement signature = SigningHelper.SignDoc(doc, cert, response.ID);

            doc.DocumentElement.InsertBefore(signature,
                doc.DocumentElement.ChildNodes[1]);

            string responseStr = doc.OuterXml;

            byte[] base64EncodedBytes =
                Encoding.UTF8.GetBytes(responseStr);

            string returnValue = System.Convert.ToBase64String(
                base64EncodedBytes);

            return returnValue;
        }
        /// <summary>
        /// GetPostSamlResponse - Returns a Base64 Encoded String with the SamlResponse in it with a Default Signature type.
        /// </summary>
        /// <param name="recipient">Recipient</param>
        /// <param name="issuer">Issuer</param>
        /// <param name="domain">Domain</param>
        /// <param name="subject">Subject</param>
        /// <param name="storeLocation">Certificate Store Location</param>
        /// <param name="storeName">Certificate Store Name</param>
        /// <param name="findType">Certificate Find Type</param>
        /// <param name="certLocation">Certificate Location</param>
        /// <param name="findValue">Certificate Find Value</param>
        /// <param name="certFile">Certificate File (used instead of the above Certificate Parameters)</param>
        /// <param name="certPassword">Certificate Password (used instead of the above Certificate Parameters)</param>
        /// <param name="attributes">A list of attributes to pass</param>
        /// <returns>A base64Encoded string with a SAML response.</returns>
        public static string GetPostSamlResponse(string recipient, string issuer, string domain, string subject,
            StoreLocation storeLocation, StoreName storeName, X509FindType findType, string certFile, string certPassword, object findValue,
            Dictionary<string, string> attributes)
        {
            return GetPostSamlResponse(recipient, issuer, domain, subject, storeLocation, storeName, findType, certFile, certPassword, findValue, attributes,
                SigningHelper.SignatureType.Response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UUID"></param>
        /// <param name="Destination"></param>
        /// <param name="ConsumerServiceURL"></param>
        /// <param name="certFile"></param>
        /// <param name="certPassword"></param>
        /// <param name="storeLocation"></param>
        /// <param name="storeName"></param>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <returns></returns>
        public static string GetPostSamlRequest(string UUID, string Destination, string ConsumerServiceURL, int SecurityLevel,
                                                string certFile, string certPassword,
                                                StoreLocation storeLocation, StoreName storeName,
                                                X509FindType findType, object findValue, string IdentityProvider, int Enviroment)
        {
            return GetPostSamlRequest(UUID, Destination, ConsumerServiceURL, SecurityLevel, certFile, certPassword,
                                      storeLocation, storeName, findType, findValue, SigningHelper.SignatureType.Request, IdentityProvider, Enviroment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UUID"></param>
        /// <param name="Destination"></param>
        /// <param name="ConsumerServiceURL"></param>
        /// <param name="certFile"></param>
        /// <param name="certPassword"></param>
        /// <param name="storeLocation"></param>
        /// <param name="storeName"></param>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <param name="signatureType"></param>
        /// <returns></returns>
        public static string GetPostSamlRequest(string UUID, string Destination, string ConsumerServiceURL, int SecurityLevel,
                                                string certFile, string certPassword,
                                                StoreLocation storeLocation, StoreName storeName,
                                                X509FindType findType, object findValue, SigningHelper.SignatureType signatureType, string IdentityProvider, int Enviroment)
        {
            AuthnRequestType MyRequest = new AuthnRequestType
            {
                ID = UUID,
                Version = "2.0"
            };
            DateTime now = DateTime.UtcNow;
            DateTime after = now.AddMinutes(10);
            string nowString = String.Empty;
            string afterString = String.Empty;
            if (IdentityProvider.Contains("sielte"))
            {
                // SIELTE
                nowString = now.AddMinutes(-2).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
                afterString = after.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
            }
            else
            {
                // POSTE - TIM - INFOCERT
                nowString = now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                afterString = after.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            }
            MyRequest.IssueInstant = nowString;
            if (SecurityLevel > 1)
            {
                MyRequest.ForceAuthn = true;
                MyRequest.ForceAuthnSpecified = true;
            }
            MyRequest.Destination = Destination;
            MyRequest.AssertionConsumerServiceIndex = (ushort)Enviroment ;
            MyRequest.AssertionConsumerServiceIndexSpecified = true;
            MyRequest.AttributeConsumingServiceIndex = 1;
            MyRequest.AttributeConsumingServiceIndexSpecified = true;

            NameIDType IssuerForRequest = new NameIDType
            {
                Value = ConsumerServiceURL.Trim(),
                Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:entity",
                NameQualifier = ConsumerServiceURL
            };
            MyRequest.Issuer = IssuerForRequest;

            NameIDPolicyType NameIdPolicyForRequest = new NameIDPolicyType
            {
                Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient",
                AllowCreate = true,
                AllowCreateSpecified = true
            };
            MyRequest.NameIDPolicy = NameIdPolicyForRequest;

            ConditionsType Conditional = new ConditionsType();
            if (IdentityProvider.Contains("sielte"))
            {
                // SIELTE
                Conditional.NotBefore = nowString;
            }
            else
            {
                // POSTE - TIM - INFOCERT
                Conditional.NotBefore = now.AddMinutes(-2).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            }
            
            Conditional.NotBeforeSpecified = true;
            Conditional.NotOnOrAfter = afterString;
            Conditional.NotOnOrAfterSpecified = true;
            MyRequest.Conditions = Conditional;

            RequestedAuthnContextType RequestedAuthn = new RequestedAuthnContextType
            {
                Comparison = AuthnContextComparisonType.minimum,
                ComparisonSpecified = true,
                ItemsElementName = new ItemsChoiceType7[] { ItemsChoiceType7.AuthnContextClassRef },
                Items = new string[] { "https://www.spid.gov.it/SpidL" + SecurityLevel.ToString() }
            };
            MyRequest.RequestedAuthnContext = RequestedAuthn;

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("saml2p", "urn:oasis:names:tc:SAML:2.0:protocol");
            //ns.Add("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");

            XmlSerializer responseSerializer = new XmlSerializer(MyRequest.GetType());

            StringWriter stringWriter = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                Encoding = Encoding.UTF8
            };

            XmlWriter responseWriter = XmlTextWriter.Create(stringWriter, settings);
            responseSerializer.Serialize(responseWriter, MyRequest, ns);
            responseWriter.Close();

            string samlString = string.Empty;
            samlString = stringWriter.ToString();

            stringWriter.Close();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(samlString);
            X509Certificate2 cert = null;
            if (System.IO.File.Exists(certFile))
            {
                cert = new X509Certificate2(certFile, certPassword);
            }
            else
            {
                X509Store store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection CertCol = store.Certificates;

                X509Certificate2Collection coll = store.Certificates.Find(findType, findValue.ToString(), false);

                if (coll.Count < 1)
                {
                    throw new ArgumentException("Unable to locate certificate");
                }
                cert = coll[0];
                store.Close();
            }

            XmlElement signature = SigningHelper.SignDoc(doc, cert, UUID);

            doc.DocumentElement.InsertBefore(signature, doc.DocumentElement.ChildNodes[1]);

            string responseStr = doc.OuterXml;

            //byte[] base64EncodedBytes =
            //    Encoding.UTF8.GetBytes(responseStr);

            //string returnValue = System.Convert.ToBase64String(
            //    base64EncodedBytes);

            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + responseStr;
        }
    }
}
