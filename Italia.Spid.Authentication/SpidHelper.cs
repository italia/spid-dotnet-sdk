/*
  Copyright (c) 2017 TEAM PER LA TRASFORMAZIONE DIGITALE

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini (see Git history for other contributors)
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Italia.Spid.Authentication.IdP;
using Italia.Spid.Authentication.Schema;

namespace Italia.Spid.Authentication
{
    public static class SpidHelper
    {
        /// <summary>
        /// Build a signed SAML authentication request.
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
        public static string BuildSpidAuthnPostRequest(string uuid, string destination, string consumerServiceURL, int securityLevel,
                                                       X509Certificate2 certificate, IdentityProvider identityProvider, int enviroment)
        {
            if (string.IsNullOrWhiteSpace(uuid))
            {
                throw new ArgumentNullException("The uuid parameter can't be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentNullException("The destination parameter can't be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(consumerServiceURL))
            {
                throw new ArgumentNullException("The consumerServiceURL parameter can't be null or empty.");
            }

            if (certificate == null)
            {
                throw new ArgumentNullException("The certificate parameter can't be null.");
            }

            if (identityProvider == null)
            {
                throw new ArgumentNullException("The identityProvider parameter can't be null.");
            }

            if (enviroment < 0 )
            {
                throw new ArgumentNullException("The enviroment parameter can't be less than zero.");
            }

            DateTime now = DateTime.UtcNow;

            AuthnRequestType authnRequest = new AuthnRequestType
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
            ns.Add("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");

            StringWriter stringWriter = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                Encoding = Encoding.UTF8
            };

            XmlWriter responseWriter = XmlTextWriter.Create(stringWriter, settings);
            XmlSerializer responseSerializer = new XmlSerializer(authnRequest.GetType());
            responseSerializer.Serialize(responseWriter, authnRequest, ns);
            responseWriter.Close();

            string samlString = stringWriter.ToString();
            stringWriter.Close();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(samlString);

            XmlElement signature = SigningHelper.SignDoc(doc, certificate, "_" + uuid);
            doc.DocumentElement.InsertBefore(signature, doc.DocumentElement.ChildNodes[1]);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + doc.OuterXml));
        }

        /// <summary>
        /// Get the IdP Authn Response and extract metadata to the returned DTO class
        /// </summary>
        /// <param name="base64Response"></param>
        /// <returns>IdpSaml2Response</returns>
        public static IdpAuthnResponse GetSpidAuthnResponse(string base64Response)
        {
            const string VALUE_NOT_AVAILABLE = "N/A";
            string idpResponse;

            if (String.IsNullOrEmpty(base64Response))
            {
                throw new ArgumentNullException("The base64Response parameter can't be null or empty.");
            }

            try
            {
                idpResponse = Encoding.UTF8.GetString(Convert.FromBase64String(base64Response));
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to converto base64 response to ascii string.", ex);
            }

            try
            {
                // Verify signature
                XmlDocument xml = new XmlDocument { PreserveWhitespace = true };
                xml.LoadXml(idpResponse);
                if (!SigningHelper.VerifySignature(xml))
                {
                    throw new Exception("Unable to verify the signature of the IdP response.");
                }

                // Parse XML document
                XDocument xdoc = new XDocument();
                xdoc = XDocument.Parse(idpResponse);

                string destination = VALUE_NOT_AVAILABLE;
                string id = VALUE_NOT_AVAILABLE;
                string inResponseTo = VALUE_NOT_AVAILABLE;
                DateTimeOffset issueInstant = DateTimeOffset.MinValue;
                string version = VALUE_NOT_AVAILABLE;
                string statusCodeValue = VALUE_NOT_AVAILABLE;
                string statusCodeInnerValue = VALUE_NOT_AVAILABLE;
                string statusMessage = VALUE_NOT_AVAILABLE;
                string statusDetail = VALUE_NOT_AVAILABLE;
                string assertionId = VALUE_NOT_AVAILABLE;
                DateTimeOffset assertionIssueInstant = DateTimeOffset.MinValue;
                string assertionVersion = VALUE_NOT_AVAILABLE;
                string assertionIssuer = VALUE_NOT_AVAILABLE;
                string subjectNameId = VALUE_NOT_AVAILABLE;
                string subjectConfirmationMethod = VALUE_NOT_AVAILABLE;
                string subjectConfirmationDataInResponseTo = VALUE_NOT_AVAILABLE;
                DateTimeOffset subjectConfirmationDataNotOnOrAfter = DateTimeOffset.MinValue;
                string subjectConfirmationDataRecipient = VALUE_NOT_AVAILABLE;
                DateTimeOffset conditionsNotBefore = DateTimeOffset.MinValue;
                DateTimeOffset conditionsNotOnOrAfter = DateTimeOffset.MinValue;
                string audience = VALUE_NOT_AVAILABLE;
                DateTimeOffset authnStatementAuthnInstant = DateTimeOffset.MinValue;
                string authnStatementSessionIndex = VALUE_NOT_AVAILABLE;
                Dictionary<string, string> spidUserInfo = new Dictionary<string, string>();

                // Extract response metadata
                XElement responseElement = xdoc.Elements("{urn:oasis:names:tc:SAML:2.0:protocol}Response").Single();
                destination = responseElement.Attribute("Destination").Value;
                id = responseElement.Attribute("ID").Value;
                inResponseTo = responseElement.Attribute("InResponseTo").Value;
                issueInstant = DateTimeOffset.Parse(responseElement.Attribute("IssueInstant").Value);
                version = responseElement.Attribute("Version").Value;

                // Extract Issuer metadata
                string issuer = responseElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}Issuer").Single().Value.Trim();

                // Extract Status metadata
                XElement StatusElement = responseElement.Descendants("{urn:oasis:names:tc:SAML:2.0:protocol}Status").Single();
                IEnumerable<XElement> statusCodeElements = StatusElement.Descendants("{urn:oasis:names:tc:SAML:2.0:protocol}StatusCode");
                statusCodeValue = statusCodeElements.First().Attribute("Value").Value.Replace("urn:oasis:names:tc:SAML:2.0:status:", "");
                statusCodeInnerValue = statusCodeElements.Count() > 1 ? statusCodeElements.Last().Attribute("Value").Value.Replace("urn:oasis:names:tc:SAML:2.0:status:", "") : VALUE_NOT_AVAILABLE;
                statusMessage = StatusElement.Elements("{urn:oasis:names:tc:SAML:2.0:protocol}StatusMessage").SingleOrDefault()?.Value ?? VALUE_NOT_AVAILABLE;
                statusDetail = StatusElement.Elements("{urn:oasis:names:tc:SAML:2.0:protocol}StatusDetail").SingleOrDefault()?.Value ?? VALUE_NOT_AVAILABLE;

                if (statusCodeValue == "Success")
                {
                    // Extract Assertion metadata
                    XElement assertionElement = responseElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}Assertion").Single();
                    assertionId = assertionElement.Attribute("ID").Value;
                    assertionIssueInstant = DateTimeOffset.Parse(assertionElement.Attribute("IssueInstant").Value);
                    assertionVersion = assertionElement.Attribute("Version").Value;
                    assertionIssuer = assertionElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}Issuer").Single().Value.Trim();

                    // Extract Subject metadata
                    XElement subjectElement = assertionElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}Subject").Single();
                    subjectNameId = subjectElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}NameID").Single().Value.Trim();
                    subjectConfirmationMethod = subjectElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}SubjectConfirmation").Single().Attribute("Method").Value;
                    XElement confirmationDataElement = subjectElement.Descendants("{urn:oasis:names:tc:SAML:2.0:assertion}SubjectConfirmationData").Single();
                    subjectConfirmationDataInResponseTo = confirmationDataElement.Attribute("InResponseTo").Value;
                    subjectConfirmationDataNotOnOrAfter = DateTimeOffset.Parse(confirmationDataElement.Attribute("NotOnOrAfter").Value);
                    subjectConfirmationDataRecipient = confirmationDataElement.Attribute("Recipient").Value;

                    // Extract Conditions metadata
                    XElement conditionsElement = assertionElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}Conditions").Single();
                    conditionsNotBefore = DateTimeOffset.Parse(conditionsElement.Attribute("NotBefore").Value);
                    conditionsNotOnOrAfter = DateTimeOffset.Parse(conditionsElement.Attribute("NotOnOrAfter").Value);
                    audience = conditionsElement.Descendants("{urn:oasis:names:tc:SAML:2.0:assertion}Audience").Single().Value.Trim();

                    // Extract AuthnStatement metadata
                    XElement authnStatementElement = assertionElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}AuthnStatement").Single();
                    authnStatementAuthnInstant = DateTimeOffset.Parse(authnStatementElement.Attribute("AuthnInstant").Value);
                    authnStatementSessionIndex = authnStatementElement.Attribute("SessionIndex").Value;

                    // Extract SPID user info
                    foreach (XElement attribute in xdoc.Descendants("{urn:oasis:names:tc:SAML:2.0:assertion}AttributeStatement").Elements())
                    {
                        spidUserInfo.Add(
                            attribute.Attribute("Name").Value,
                            attribute.Elements().Single(a => a.Name == "{urn:oasis:names:tc:SAML:2.0:assertion}AttributeValue").Value.Trim()
                        );
                    }
                }

                return new IdpAuthnResponse(destination, id, inResponseTo, issueInstant, version, issuer,
                                            statusCodeValue, statusCodeInnerValue, statusMessage, statusDetail,
                                            assertionId, assertionIssueInstant, assertionVersion, assertionIssuer,
                                            subjectNameId, subjectConfirmationMethod, subjectConfirmationDataInResponseTo,
                                            subjectConfirmationDataNotOnOrAfter, subjectConfirmationDataRecipient,
                                            conditionsNotBefore, conditionsNotOnOrAfter, audience,
                                            authnStatementAuthnInstant, authnStatementSessionIndex,
                                            spidUserInfo);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to read AttributeStatement attributes from SAML2 document.", ex);
            }
        }

        /// <summary>
        /// Check the validity of IdP authentication response
        /// </summary>
        /// <param name="idpAuthnResponse"></param>
        /// <param name="spidRequestId"></param>
        /// <param name="route"></param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidAuthnResponse(IdpAuthnResponse idpAuthnResponse, string spidRequestId, string route)
        {
            return (idpAuthnResponse.InResponseTo == "_" + spidRequestId) && (idpAuthnResponse.SubjectConfirmationDataRecipient == route);
        }

        /// <summary>
        /// Build a signed SAML logout request.
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="destination"></param>
        /// <param name="consumerServiceURL"></param>
        /// <param name="certificate"></param>
        /// <param name="identityProvider"></param>
        /// <param name="subjectNameId"></param>
        /// <param name="authnStatementSessionIndex"></param>
        /// <returns></returns>
        public static string BuildSpidLogoutPostRequest(string uuid, string consumerServiceURL, X509Certificate2 certificate,
                                                        IdentityProvider identityProvider, string subjectNameId, string authnStatementSessionIndex)
        {
            if (string.IsNullOrWhiteSpace(uuid))
            {
                throw new ArgumentNullException("The uuid parameter can't be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(consumerServiceURL))
            {
                throw new ArgumentNullException("The consumerServiceURL parameter can't be null or empty.");
            }

            if (certificate == null)
            {
                throw new ArgumentNullException("The certificate parameter can't be null.");
            }

            if (identityProvider == null)
            {
                throw new ArgumentNullException("The identityProvider parameter can't be null.");
            }

            if (string.IsNullOrWhiteSpace(subjectNameId))
            {
                throw new ArgumentNullException("The subjectNameId parameter can't be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(identityProvider.LogoutServiceUrl))
            {
                throw new ArgumentNullException("The LogoutServiceUrl of the identity provider is null or empty.");
            }
            
            DateTime now = DateTime.UtcNow;

            LogoutRequestType logoutRequest = new LogoutRequestType
            {
                ID = "_" + uuid,
                Version = "2.0",
                IssueInstant = identityProvider.Now(now),
                Destination = identityProvider.LogoutServiceUrl,
                Issuer = new NameIDType
                {
                    Value = consumerServiceURL.Trim(),
                    Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:entity",
                    NameQualifier = consumerServiceURL
                },
                Item = new NameIDType
                {
                    SPNameQualifier = consumerServiceURL,
                    Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient",
                    Value = identityProvider.SubjectNameIdFormatter(subjectNameId)
                },
                NotOnOrAfterSpecified = true,
                NotOnOrAfter = now.AddMinutes(10),
                Reason = "urn:oasis:names:tc:SAML:2.0:logout:user",
                SessionIndex = new string[] { authnStatementSessionIndex }
            };

            try
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("saml2p", "urn:oasis:names:tc:SAML:2.0:protocol");
                ns.Add("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");

                StringWriter stringWriter = new StringWriter();
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    Encoding = Encoding.UTF8
                };

                XmlWriter responseWriter = XmlTextWriter.Create(stringWriter, settings);
                XmlSerializer responseSerializer = new XmlSerializer(logoutRequest.GetType());
                responseSerializer.Serialize(responseWriter, logoutRequest, ns);
                responseWriter.Close();

                string samlString = stringWriter.ToString();
                stringWriter.Close();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(samlString);

                XmlElement signature = SigningHelper.SignDoc(doc, certificate, "_" + uuid);
                doc.DocumentElement.InsertBefore(signature, doc.DocumentElement.ChildNodes[1]);

                return Convert.ToBase64String(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + doc.OuterXml));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the IdP Logout Response and extract metadata to the returned DTO class
        /// </summary>
        /// <param name="base64Response"></param>
        /// <returns></returns>
        public static IdpLogoutResponse GetSpidLogoutResponse(string base64Response)
        {
            const string VALUE_NOT_AVAILABLE = "N/A";
            string idpResponse;

            if (String.IsNullOrEmpty(base64Response))
            {
                throw new ArgumentNullException("The base64Response parameter can't be null or empty.");
            }

            try
            {
                idpResponse = Encoding.UTF8.GetString(Convert.FromBase64String(base64Response));
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to converto base64 response to ascii string.", ex);
            }

            try
            {
                // Verify signature
                XmlDocument xml = new XmlDocument { PreserveWhitespace = true };
                xml.LoadXml(idpResponse);
                if (!SigningHelper.VerifySignature(xml))
                {
                    throw new Exception("Unable to verify the signature of the IdP response.");
                }

                // Parse XML document
                XDocument xdoc = new XDocument();
                xdoc = XDocument.Parse(idpResponse);

                string destination = VALUE_NOT_AVAILABLE;
                string id = VALUE_NOT_AVAILABLE;
                string inResponseTo = VALUE_NOT_AVAILABLE;
                DateTimeOffset issueInstant = DateTimeOffset.MinValue;
                string version = VALUE_NOT_AVAILABLE;
                string statusCodeValue = VALUE_NOT_AVAILABLE;
                string statusCodeInnerValue = VALUE_NOT_AVAILABLE;
                string statusMessage = VALUE_NOT_AVAILABLE;
                string statusDetail = VALUE_NOT_AVAILABLE;

                // Extract response metadata
                XElement responseElement = xdoc.Elements("{urn:oasis:names:tc:SAML:2.0:protocol}LogoutResponse").Single();
                destination = responseElement.Attribute("Destination").Value;
                id = responseElement.Attribute("ID").Value;
                inResponseTo = responseElement.Attribute("InResponseTo").Value;
                issueInstant = DateTimeOffset.Parse(responseElement.Attribute("IssueInstant").Value);
                version = responseElement.Attribute("Version").Value;

                // Extract Issuer metadata
                string issuer = responseElement.Elements("{urn:oasis:names:tc:SAML:2.0:assertion}Issuer").Single().Value.Trim();

                // Extract Status metadata
                XElement StatusElement = responseElement.Descendants("{urn:oasis:names:tc:SAML:2.0:protocol}Status").Single();
                IEnumerable<XElement> statusCodeElements = StatusElement.Descendants("{urn:oasis:names:tc:SAML:2.0:protocol}StatusCode");
                statusCodeValue = statusCodeElements.First().Attribute("Value").Value.Replace("urn:oasis:names:tc:SAML:2.0:status:", "");
                statusCodeInnerValue = statusCodeElements.Count() > 1 ? statusCodeElements.Last().Attribute("Value").Value.Replace("urn:oasis:names:tc:SAML:2.0:status:", "") : VALUE_NOT_AVAILABLE;
                statusMessage = StatusElement.Elements("{urn:oasis:names:tc:SAML:2.0:protocol}StatusMessage").SingleOrDefault()?.Value ?? VALUE_NOT_AVAILABLE;
                statusDetail = StatusElement.Elements("{urn:oasis:names:tc:SAML:2.0:protocol}StatusDetail").SingleOrDefault()?.Value ?? VALUE_NOT_AVAILABLE;

                return new IdpLogoutResponse(destination, id, inResponseTo, issueInstant, version, issuer,
                                             statusCodeValue, statusCodeInnerValue, statusMessage, statusDetail);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to read AttributeStatement attributes from SAML2 document.", ex);
            }
        }

        /// <summary>
        /// Check the validity of IdP logout response
        /// </summary>
        /// <param name="idpLogoutResponse"></param>
        /// <param name="spidRequestId"></param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidLogoutResponse(IdpLogoutResponse idpLogoutResponse, string spidRequestId)
        {
            return (idpLogoutResponse.InResponseTo == "_" + spidRequestId);
        }

    }
}
