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
using TPCWare.Spid.Sdk.Schema;
using log4net;
using TPCWare.Spid.Sdk.IdP;
using System.Xml.Linq;

namespace TPCWare.Spid.Sdk
{
    public static class Saml2Helper
    {
        static ILog log = log4net.LogManager.GetLogger(typeof(Saml2Helper));

        /// <summary>
        /// Build a signed SAML request.
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="destination"></param>
        /// <param name="consumerServiceURL"></param>
        /// <param name="securityLevel"></param>
        /// <param name="certFile"></param>
        /// <param name="certPassword"></param>
        /// <param name="storeLocation"></param>
        /// <param name="storeName"></param>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <param name="identityProvider"></param>
        /// <param name="enviroment"></param>
        /// <returns>Returns a Base64 Encoded String of the SAML request</returns>
        public static string BuildPostSamlRequest(string uuid, string destination, string consumerServiceURL, int securityLevel,
                                                  X509Certificate2 certificate, IdentityProvider identityProvider, int enviroment)
        {
            if (string.IsNullOrWhiteSpace(uuid))
            {
                log.Error("Error on BuildPostSamlRequest: The uuid parameter is null or empty.");
                throw new ArgumentNullException("The uuid parameter can't be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(destination))
            {
                log.Error("Error on BuildPostSamlRequest: The destination parameter is null or empty.");
                throw new ArgumentNullException("The destination parameter can't be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(consumerServiceURL))
            {
                log.Error("Error on BuildPostSamlRequest: The consumerServiceURL parameter is null or empty.");
                throw new ArgumentNullException("The consumerServiceURL parameter can't be null or empty.");
            }

            if (certificate == null)
            {
                log.Error("Error on BuildPostSamlRequest: The certificate parameter is null.");
                throw new ArgumentNullException("The certificate parameter can't be null.");
            }

            if (identityProvider == null)
            {
                log.Error("Error on BuildPostSamlRequest: The identityProvider parameter is null.");
                throw new ArgumentNullException("The identityProvider parameter can't be null.");
            }

            if (enviroment < 0 )
            {
                log.Error("Error on BuildPostSamlRequest: The enviroment parameter is less than zero.");
                throw new ArgumentNullException("The enviroment parameter can't be less than zero.");
            }

            DateTime now = DateTime.UtcNow;

            AuthnRequestType MyRequest = new AuthnRequestType
            {
                ID = "_" + uuid,
                Version = "2.0",
                IssueInstant = identityProvider.Now(now),
                Destination = destination,
                AssertionConsumerServiceIndex = (ushort)enviroment,
                AssertionConsumerServiceIndexSpecified = true,
                AttributeConsumingServiceIndex = 1,
                AttributeConsumingServiceIndexSpecified = true,
                ForceAuthn = (securityLevel > 1),
                ForceAuthnSpecified = (securityLevel > 1),
                Issuer = new NameIDType
                {
                    Value = consumerServiceURL.Trim(),
                    Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:entity",
                    NameQualifier = consumerServiceURL
                },
                NameIDPolicy = new NameIDPolicyType
                {
                    Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient",
                    AllowCreate = true,
                    AllowCreateSpecified = true
                },
                Conditions = new ConditionsType
                {
                    NotBefore = identityProvider.NotBefore(now),
                    NotBeforeSpecified = true,
                    NotOnOrAfter = identityProvider.After(now.AddMinutes(10)),
                    NotOnOrAfterSpecified = true
                },
                RequestedAuthnContext = new RequestedAuthnContextType
                {
                    Comparison = AuthnContextComparisonType.minimum,
                    ComparisonSpecified = true,
                    ItemsElementName = new ItemsChoiceType7[] { ItemsChoiceType7.AuthnContextClassRef },
                    Items = new string[] { "https://www.spid.gov.it/SpidL" + securityLevel.ToString() }
                }
            };

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("saml2p", "urn:oasis:names:tc:SAML:2.0:protocol");


            StringWriter stringWriter = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                Encoding = Encoding.UTF8
            };

            XmlWriter responseWriter = XmlTextWriter.Create(stringWriter, settings);
            XmlSerializer responseSerializer = new XmlSerializer(MyRequest.GetType());
            responseSerializer.Serialize(responseWriter, MyRequest, ns);
            responseWriter.Close();

            string samlString = stringWriter.ToString();
            stringWriter.Close();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(samlString);

            XmlElement signature = SigningHelper.SignDoc(doc, certificate, "_" + uuid);

            doc.DocumentElement.InsertBefore(signature, doc.DocumentElement.ChildNodes[1]);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + doc.OuterXml));
        }

        public static IdpSaml2Response GetIdpSaml2Response(string base64Response)
        {
            string idpAsciiResponse;

            if (String.IsNullOrEmpty(base64Response))
            {
                log.Error("Error on GetSpidUserInfo: The base64Response parameter is null or empty.");
                throw new ArgumentNullException("The base64Response parameter can't be null or empty.");
            }

            try
            {
                idpAsciiResponse = Encoding.UTF8.GetString(Convert.FromBase64String(base64Response));
            }
            catch (Exception ex)
            {
                log.Error("Error on GetSpidUserInfo: Unable to convert base64 response to ascii string.");
                throw new ArgumentException("Unable to converto base64 response to ascii string.", ex);
            }

            try
            {
                // Verify signature
                XmlDocument xml = new XmlDocument { PreserveWhitespace = true };
                xml.LoadXml(idpAsciiResponse);
                if (!SigningHelper.VerifySignature(xml))
                {
                    log.Error("Error on GetSpidUserInfo: Unable to verify the signature of the IdP response.");
                    throw new Exception("Unable to verify the signature of the IdP response.");
                }

                // Parse XML document
                XDocument xdoc = new XDocument();
                xdoc = XDocument.Parse(idpAsciiResponse);

                // Extract response metadata
                XElement responseElement = xdoc.Elements("{urn:oasis:names:tc:SAML:2.0:protocol}Response").Single();
                string destination = responseElement.Attribute("Destination").Value;
                string id = responseElement.Attribute("ID").Value;
                string inResponseTo = responseElement.Attribute("InResponseTo").Value;
                DateTimeOffset issueInstant = DateTimeOffset.Parse(responseElement.Attribute("IssueInstant").Value);
                string version = responseElement.Attribute("Version").Value;

                // Extract Issuer metadata
                string issuer = responseElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}Issuer").Single().Value.Trim();

                // Extract Status metadata
                string statusCodeValue = responseElement.Descendants("{urn:oasis:names:tc:SAML:2.0:protocol}StatusCode")
                                                        .Single().Attribute("Value").Value
                                                        .Replace("urn:oasis:names:tc:SAML:2.0:status:", "");

                // Extract Assertion
                XElement assertionElement = responseElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}Assertion").Single();
                string assertionId = assertionElement.Attribute("ID").Value;
                DateTimeOffset assertionIssueInstant = DateTimeOffset.Parse(assertionElement.Attribute("IssueInstant").Value);
                string assertionVersion = assertionElement.Attribute("Version").Value;
                string assertionIssuer = assertionElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}Issuer").Single().Value.Trim();

                // Extract Subject metadata
                XElement subjectElement = assertionElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}Subject").Single();
                string subjectNameId = subjectElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}NameID").Single().Value.Trim();
                string subjectConfirmationMethod = subjectElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}SubjectConfirmation").Single().Attribute("Method").Value;
                XElement confirmationDataElement = subjectElement.Descendants("{urn:oasis:names:tc:SAML:2.0:assertion}SubjectConfirmationData").Single();
                string subjectConfirmationDataInResponseTo = confirmationDataElement.Attribute("InResponseTo").Value;
                DateTimeOffset subjectConfirmationDataNotOnOrAfter = DateTimeOffset.Parse(confirmationDataElement.Attribute("NotOnOrAfter").Value);
                string subjectConfirmationDataRecipient = confirmationDataElement.Attribute("Recipient").Value;

                // Extract Conditions metadata
                XElement conditionsElement = assertionElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}Conditions").Single();
                DateTimeOffset conditionsNotBefore = DateTimeOffset.Parse(conditionsElement.Attribute("NotBefore").Value);
                DateTimeOffset conditionsNotOnOrAfter = DateTimeOffset.Parse(conditionsElement.Attribute("NotOnOrAfter").Value);
                string audience = conditionsElement.Descendants("{urn:oasis:names:tc:SAML:2.0:assertion}Audience").Single().Value.Trim();

                // Extract AuthnStatement metadata
                XElement authnStatementElement = assertionElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}AuthnStatement").Single();
                DateTimeOffset authnStatementAuthnInstant = DateTimeOffset.Parse(authnStatementElement.Attribute("AuthnInstant").Value);
                string authnStatementSessionIndex = authnStatementElement.Attribute("SessionIndex").Value;

                // Extract SPID user info
                Dictionary<string, string> spidUserInfo = new Dictionary<string, string>();
                foreach (XElement attribute in xdoc.Descendants("{urn:oasis:names:tc:SAML:2.0:assertion}AttributeStatement").Elements())
                {
                    spidUserInfo.Add(
                        attribute.Attribute("Name").Value,
                        attribute.Elements().Single(a => a.Name == "{urn:oasis:names:tc:SAML:2.0:assertion}AttributeValue").Value.Trim()
                    );
                }

                return new IdpSaml2Response(destination, id, inResponseTo, issueInstant, version, issuer, statusCodeValue,
                                            assertionId, assertionIssueInstant, assertionVersion, assertionIssuer,
                                            subjectNameId, subjectConfirmationMethod, subjectConfirmationDataInResponseTo,
                                            subjectConfirmationDataNotOnOrAfter, subjectConfirmationDataRecipient,
                                            conditionsNotBefore, conditionsNotOnOrAfter, audience,
                                            authnStatementAuthnInstant, authnStatementSessionIndex,
                                            spidUserInfo);
            }
            catch (Exception ex)
            {
                log.Error("Error on GetSpidUserInfo: Unable to read metadata from SAML2 document (see raw response).");
                log.Error("RAW RESPONSE: " + idpAsciiResponse);
                throw new ArgumentException("Unable to read AttributeStatement attributes from SAML2 document.", ex);
            }
        }

    }
}
