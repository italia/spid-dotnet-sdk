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

namespace TPCWare.Spid.Sdk
{
    public static class Saml2Helper
    {
        static ILog log = log4net.LogManager.GetLogger(typeof(Saml2Helper));

        /// <summary>
        /// Creates a Version 2.0 Saml Assertion
        /// </summary>
        /// <param name="issuer"></param>
        /// <param name="recipient"></param>
        /// <param name="domain"></param>
        /// <param name="subject"></param>
        /// <param name="attributes"></param>
        /// <returns>returns a Version 2.0 Saml Assertion</returns>
        private static AssertionType CreateSamlAssertion(string issuer, string recipient, string domain, string subject, Dictionary<string, string> attributes)
        {
            // WARNING: the recipient is not used!
            // TODO: Verify that the recipient can be ignored

            if (string.IsNullOrWhiteSpace(issuer))
            {
                log.Error("Error on CreateSamlAssertion: The issuer parameter is null or empty.");
                throw new ArgumentNullException("The issuer parameter can't be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(recipient))
            {
                log.Error("Error on CreateSamlAssertion: The recipient parameter is null or empty.");
                throw new ArgumentNullException("The recipient parameter can't be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(domain))
            {
                log.Error("Error on CreateSamlAssertion: The domain parameter is null or empty.");
                throw new ArgumentNullException("The domain parameter can't be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                log.Error("Error on CreateSamlAssertion: The subject parameter is null or empty.");
                throw new ArgumentNullException("The subject parameter can't be null or empty.");
            }

            if (attributes == null)
            {
                log.Error("Error on CreateSamlAssertion: The attributes parameter is null.");
                throw new ArgumentNullException("The attributes parameter can't be null.");
            }

            DateTime now = DateTime.UtcNow;

            return new AssertionType
            {
                Conditions = new ConditionsType
                {
                    Items = new ConditionAbstractType[]
                    {
                        new AudienceRestrictionType
                        {
                            Audience = new string[] { domain.Trim() }
                        }
                    },
                    NotBefore = now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                    NotBeforeSpecified = true,
                    NotOnOrAfter = now.AddMinutes(10).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                    NotOnOrAfterSpecified = true
                },
                ID = "_" + Guid.NewGuid().ToString(),
                IssueInstant = now,
                Issuer = new NameIDType
                {
                    Value = issuer.Trim()
                },
                Items = new StatementAbstractType[]
                {
                    new AuthnStatementType
                    {
                        AuthnInstant = now,
                        AuthnContext = new AuthnContextType
                        {
                            ItemsElementName = new ItemsChoiceType5[] { ItemsChoiceType5.AuthnContextClassRef },
                            Items = new object[] { "AuthnContextClassRef" }
                        }
                    },
                    new AttributeStatementType()
                    {
                        Items = attributes.Select(a => new AttributeType
                        {
                            Name = a.Key,
                            NameFormat = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic",
                            AttributeValue = new object[] { a.Value }
                        }).ToArray()
                    }
                },
                Subject = new SubjectType
                {
                    Items = new object[]
                    {
                        //Name Identifier to be used in Saml Subject
                        new NameIDType
                        {
                            NameQualifier = domain.Trim(),
                            Value = subject.Trim()
                        },
                        new SubjectConfirmationType
                        {
                            Method = "urn:oasis:names:tc:SAML:2.0:cm:bearer",
                            SubjectConfirmationData = new SubjectConfirmationDataType()
                        }
                    }
                },
                Version = "2.0"
            };
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
        public static string BuildPostSamlResponse(string recipient, string issuer, string domain, string subject,
            StoreLocation storeLocation, StoreName storeName, X509FindType findType, string certFile, string certPassword, object findValue,
            Dictionary<string, string> attributes)
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

            AssertionType assertionType = CreateSamlAssertion(
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
            if (File.Exists(certFile))
            {
                cert = new X509Certificate2(certFile, certPassword);
            }
            else
            {
                X509Store store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection CertCol = store.Certificates;
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
    }
}
