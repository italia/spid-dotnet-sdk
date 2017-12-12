using Developers.Italia.SPID.SAML.Schema;
using System;
using System.Collections.Generic;
using System.IO;


using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Developers.Italia.SPID.SAML
{
    public class AuthResponse
    {
        public string Version { get; set; }

        public string Issuer { get; set; }

        public string UUID { get; set; }


        public string SPUID { get; set; }

        public string SubjectNameId { get; set; }

        public string SessionId { get; set; }

        public DateTime SessionIdExpireDate { get; set; }
              
        public SpidUserData User { get; set; }

        public SamlRequestStatus RequestStatus { get; set; }


        public AuthResponse()
        {
            User = new SpidUserData();
            RequestStatus= SamlRequestStatus.GenericError;
        }

        public Dictionary<string, string> GetClaims()
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();


            claims.Add("SpidCode", this.User.SpidCode ?? "");

            claims.Add("Name", this.User.Name ?? "");
            claims.Add("FamilyName", this.User.FamilyName ?? "");

            claims.Add("PlaceOfBirth", this.User.PlaceOfBirth ?? "");
            claims.Add("CountyOfBirth", this.User.CountyOfBirth ?? "");
            claims.Add("DateOfBirth", this.User.DateOfBirth ?? "");

            claims.Add("Gender", this.User.Gender ?? "");

            claims.Add("CompanyName", this.User.CompanyName ?? "");
            claims.Add("RegisteredOffice", this.User.RegisteredOffice ?? "");
            claims.Add("FiscalNumber", this.User.FiscalNumber ?? "");
            claims.Add("IvaCode", this.User.IvaCode ?? "");

            claims.Add("IdCard", this.User.IdCard ?? "");
            claims.Add("ExpirationDate", this.User.ExpirationDate ?? "");

            claims.Add("Email", this.User.Email ?? "");
            claims.Add("Address", this.User.Address ?? "");
            claims.Add("DigitalAddress", this.User.DigitalAddress ?? "");
            claims.Add("MobilePhone", this.User.MobilePhone ?? "");

            claims.Add("SessionId", this.SessionId ?? "");
            claims.Add("SessionIdExpireDate", this.SessionIdExpireDate.ToString());
            claims.Add("SubjectNameId", this.SubjectNameId ?? "");

            return claims;

        }


        public void Deserialize(string samlResponse)
        {
            ResponseType response = new ResponseType();
            try
            {
                using (TextReader sr = new StringReader(samlResponse))
                {
                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ResponseType));
                    response = (ResponseType)serializer.Deserialize(sr);

                    this.Version = response.Version;
                    this.UUID = response.ID;
                    this.SPUID = response.InResponseTo;
                    this.Issuer = response.Issuer.Value;

                    switch (response.Status.StatusCode.Value)
                    {
                        case "urn:oasis:names:tc:SAML:2.0:status:Success":
                            this.RequestStatus = SamlRequestStatus.Success;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:Requester":
                            this.RequestStatus = SamlRequestStatus.RequesterError;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:Responder":
                            this.RequestStatus = SamlRequestStatus.ResponderError;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:VersionMismatch":
                            this.RequestStatus = SamlRequestStatus.VersionMismatchError;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:AuthnFailed":
                            this.RequestStatus = SamlRequestStatus.AuthnFailed;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:InvalidAttrNameOrValue":
                            this.RequestStatus = SamlRequestStatus.InvalidAttrNameOrValue;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:InvalidNameIDPolicy":
                            this.RequestStatus = SamlRequestStatus.InvalidNameIDPolicy;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:NoAuthnContext":
                            this.RequestStatus = SamlRequestStatus.NoAuthnContext;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:NoAvailableIDP":
                            this.RequestStatus = SamlRequestStatus.NoAvailableIDP;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:NoPassive":
                            this.RequestStatus = SamlRequestStatus.NoPassive;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:NoSupportedIDP":
                            this.RequestStatus = SamlRequestStatus.NoSupportedIDP;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:PartialLogout":
                            this.RequestStatus = SamlRequestStatus.PartialLogout;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:ProxyCountExceeded":
                            this.RequestStatus = SamlRequestStatus.ProxyCountExceeded;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:RequestDenied":
                            this.RequestStatus = SamlRequestStatus.RequestDenied;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:RequestUnsupported":
                            this.RequestStatus = SamlRequestStatus.RequestUnsupported;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:RequestVersionDeprecated":
                            this.RequestStatus = SamlRequestStatus.RequestVersionDeprecated;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:RequestVersionTooHigh":
                            this.RequestStatus = SamlRequestStatus.RequestVersionTooHigh;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:RequestVersionTooLow":
                            this.RequestStatus = SamlRequestStatus.RequestVersionTooLow;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:ResourceNotRecognized":
                            this.RequestStatus = SamlRequestStatus.ResourceNotRecognized;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:TooManyResponses":
                            this.RequestStatus = SamlRequestStatus.TooManyResponses;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:UnknownAttrProfile":
                            this.RequestStatus = SamlRequestStatus.UnknownAttrProfile;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:UnknownPrincipal":
                            this.RequestStatus = SamlRequestStatus.UnknownPrincipal;
                            break;
                        case "urn:oasis:names:tc:SAML:2.0:status:UnsupportedBinding":
                            this.RequestStatus = SamlRequestStatus.UnsupportedBinding;
                            break;

                        default:
                            this.RequestStatus = SamlRequestStatus.GenericError;
                            break;

                    }

                    if (this.RequestStatus == SamlRequestStatus.Success) {

                        foreach (var item in response.Items)
                        {
                            if (item.GetType()==typeof(AssertionType)) {
                                AssertionType ass = (AssertionType)item;
                                this.SessionIdExpireDate = (ass.Conditions.NotOnOrAfter != null) ? ass.Conditions.NotOnOrAfter : DateTime.Now.AddMinutes(20);

                                foreach (var subitem in ass.Subject.Items)
                                {
                                    if (subitem.GetType() == typeof(NameIDType))
                                    {
                                        NameIDType nameId = (NameIDType)subitem;
                                        this.SubjectNameId = nameId.Value; //.Replace("SPID-","");
                                    }
                                }

                                foreach (var assItem in ass.Items)
                                {
                                    if (assItem.GetType() == typeof(AuthnStatementType))
                                    {
                                        AuthnStatementType authnStatement = (AuthnStatementType)assItem;
                                        this.SessionId = authnStatement.SessionIndex;
                                        this.SessionIdExpireDate = (authnStatement.SessionNotOnOrAfterSpecified) ? authnStatement.SessionNotOnOrAfter : this.SessionIdExpireDate;
                                    }

                                    if (assItem.GetType() == typeof(AttributeStatementType))
                                    {
                                        AttributeStatementType statement = (AttributeStatementType)assItem;
                                        
                                        foreach (AttributeType attribute in statement.Items)
                                        {
                                            switch (attribute.Name)
                                            {
                                                case "spidCode":
                                                    this.User.SpidCode = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "name":
                                                    this.User.Name = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "familyName":
                                                    this.User.FamilyName = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "gender":
                                                    this.User.Gender  = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "ivaCode":
                                                    this.User.IvaCode = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "companyName":
                                                    this.User.CompanyName = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "mobilePhone":
                                                    this.User.MobilePhone = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "address":
                                                    this.User.Address = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "fiscalNumber":
                                                    this.User.FiscalNumber = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "dateOfBirth":
                                                    this.User.DateOfBirth = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "placeOfBirth":
                                                    this.User.PlaceOfBirth = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "countyOfBirth":
                                                    this.User.CountyOfBirth = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "idCard":
                                                    this.User.IdCard = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "registeredOffice":
                                                    this.User.RegisteredOffice = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "email":
                                                    this.User.Email = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "expirationDate":
                                                    this.User.ExpirationDate = attribute.AttributeValue[0].ToString();
                                                    break;
                                                case "digitalAddress":
                                                    this.User.DigitalAddress = attribute.AttributeValue[0].ToString();
                                                    break;
                                             
                                                default:
                                                    break;
                                            }
                                        }

                                    }
                                }
                            }
                        }
                      

                    }

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }






        }


        /// <summary>
        /// 
        /// </summary>
        public enum SamlRequestStatus
        {
            /// <summary>
            /// The request succeeded.
            /// </summary>
            Success,

            GenericError,

            /// <summary>
            /// The request could not be performed due to an error on the part of the requester.
            /// </summary>
            RequesterError,

            /// <summary>
            /// The request could not be performed due to an error on the part of the SAML responder or SAML authority.
            /// </summary>
            ResponderError,

            /// <summary>
            /// The SAML responder could not process the request because the version of the request message was incorrect.
            /// </summary>
            VersionMismatchError,


            /// <summary>
            /// The responding provider was unable to successfully authenticate the principal.
            /// </summary>
            AuthnFailed,

            /// <summary>
            /// Unexpected or invalid content was encountered within a <saml:Attribute> or <saml:AttributeValue> element.
            /// </summary>
            InvalidAttrNameOrValue,

            /// <summary>
            /// The responding provider cannot or will not support the requested name identifier policy.
            /// </summary>
            InvalidNameIDPolicy,

            /// <summary>
            /// The specified authentication context requirements cannot be met by the responder.
            /// </summary>
            NoAuthnContext,

            /// <summary>
            /// Used by an intermediary to indicate that none of the supported identity provider <Loc> elements in an <IDPList> can be resolved or that none of the supported identity providers are available.
            /// </summary>
            NoAvailableIDP,

            /// <summary>
            /// Indicates that the responding provider cannot authenticate the principal passively, as has been requested.
            /// </summary>
            NoPassive,

            /// <summary>
            /// Used by an intermediary to indicate that none of the identity providers in an <IDPList> are supported by the intermediary.
            /// </summary>
            NoSupportedIDP,

            /// <summary>
            /// Used by a session authority to indicate to a session participant that it was not able to propagate the logout request to all other session participants.
            /// </summary>
            PartialLogout,

            /// <summary>
            /// Indicates that a responding provider cannot authenticate the principal directly and is not permitted to proxy the request further.
            /// </summary>
            ProxyCountExceeded,

            /// <summary>
            /// The SAML responder or SAML authority is able to process the request but has chosen not to respond. This status code MAY be used when there is concern about the security context of the request message or the sequence of request messages received from a particular requester.
            /// </summary>
            RequestDenied,

            /// <summary>
            /// The SAML responder or SAML authority does not support the request.
            /// </summary>
            RequestUnsupported,

            /// <summary>
            /// The SAML responder cannot process any requests with the protocol version specified in the request.
            /// </summary>
            RequestVersionDeprecated,

            /// <summary>
            /// The SAML responder cannot process the request because the protocol version specified in the request message is a major upgrade from the highest protocol version supported by the responder.
            /// </summary>
            RequestVersionTooHigh,

            /// <summary>
            /// The SAML responder cannot process the request because the protocol version specified in the request message is too low.
            /// </summary>
            RequestVersionTooLow,

            /// <summary>
            /// The resource value provided in the request message is invalid or unrecognized.
            /// </summary>
            ResourceNotRecognized,

            /// <summary>
            /// The response message would contain more elements than the SAML responder is able to return.
            /// </summary>
            TooManyResponses,

            /// <summary>
            /// An entity that has no knowledge of a particular attribute profile has been presented with an attribute drawn from that profile.
            /// </summary>
            UnknownAttrProfile,

            /// <summary>
            /// The responding provider does not recognize the principal specified or implied by the request.
            /// </summary>
            UnknownPrincipal,

            /// <summary>
            /// The SAML responder cannot properly fulfill the request using the protocol binding specified in the request.
            /// </summary>
            UnsupportedBinding
        }
    }
}
