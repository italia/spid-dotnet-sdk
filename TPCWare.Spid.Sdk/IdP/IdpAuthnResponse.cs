using System;
using System.Collections.Generic;
using System.Text;

namespace TPCWare.Spid.Sdk.IdP
{
    public class IdpAuthnResponse
    {
        public string Destination { get; private set; }

        public string Id { get; private set; }

        public string InResponseTo { get; private set; }

        public DateTimeOffset IssueInstant { get; private set; }

        public string Version { get; private set; }

        public string Issuer { get; private set; }

        public string StatusCodeValue { get; private set; }

        public string StatusCodeInnerValue { get; private set; }

        public string StatusMessage { get; private set; }

        public string StatusDetail { get; private set; }

        public string AssertionId { get; private set; }

        public DateTimeOffset AssertionIssueInstant { get; private set; }

        public string AssertionVersion { get; private set; }

        public string AssertionIssuer { get; private set; }

        public string SubjectNameId { get; private set; }

        public string SubjectConfirmationMethod { get; private set; }

        public string SubjectConfirmationDataInResponseTo { get; private set; }

        public DateTimeOffset SubjectConfirmationDataNotOnOrAfter { get; private set; }

        public string SubjectConfirmationDataRecipient { get; private set; }

        public DateTimeOffset ConditionsNotBefore { get; private set; }

        public DateTimeOffset ConditionsNotOnOrAfter { get; private set; }

        public string Audience { get; private set; }

        public DateTimeOffset AuthnStatementAuthnInstant { get; private set; }

        public string AuthnStatementSessionIndex { get; private set; }

        public Dictionary<string, string> SpidUserInfo { get; private set; }

        public bool IsSuccessful
        {
            get { return StatusCodeValue == "Success"; }
        }

        public IdpAuthnResponse(string destination, string id, string inResponseTo, DateTimeOffset issueInstant, string version, string issuer,
                                string statusCodeValue, string statusCodeInnerValue, string statusMessage, string statusDetail,
                                string assertionId, DateTimeOffset assertionIssueInstant, string assertionVersion, string assertionIssuer,
                                string subjectNameId, string subjectConfirmationMethod, string subjectConfirmationDataInResponseTo,
                                DateTimeOffset subjectConfirmationDataNotOnOrAfter, string subjectConfirmationDataRecipient,
                                DateTimeOffset conditionsNotBefore, DateTimeOffset conditionsNotOnOrAfter, string audience,
                                DateTimeOffset authnStatementAuthnInstant, string authnStatementSessionIndex,
                                Dictionary<string, string> spidUserInfo)
        {
            Destination = destination;
            Id = id;
            InResponseTo = inResponseTo;
            IssueInstant = issueInstant;
            Version = version;
            Issuer =issuer ;
            StatusCodeValue = statusCodeValue;
            StatusCodeInnerValue = statusCodeInnerValue;
            StatusMessage = statusMessage;
            StatusDetail = statusDetail;
            AssertionId = assertionId;
            AssertionIssueInstant = AssertionIssueInstant;
            AssertionVersion = assertionVersion;
            AssertionIssuer = assertionIssuer;
            SubjectNameId = subjectNameId;
            SubjectConfirmationMethod = subjectConfirmationMethod;
            SubjectConfirmationDataInResponseTo = subjectConfirmationDataInResponseTo;
            SubjectConfirmationDataNotOnOrAfter = subjectConfirmationDataNotOnOrAfter;
            SubjectConfirmationDataRecipient = subjectConfirmationDataRecipient;
            ConditionsNotBefore = conditionsNotBefore;
            ConditionsNotOnOrAfter = conditionsNotOnOrAfter;
            Audience = audience;
            AuthnStatementAuthnInstant = authnStatementAuthnInstant;
            AuthnStatementSessionIndex = authnStatementSessionIndex;
            SpidUserInfo = spidUserInfo;
        }
    }
}