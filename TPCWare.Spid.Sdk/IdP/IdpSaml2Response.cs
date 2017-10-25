using System;
using System.Collections.Generic;
using System.Text;

namespace TPCWare.Spid.Sdk.IdP
{
    public class IdpSaml2Response
    {
        public string Destination { get; private set; }

        public string Id { get; private set; }

        public string InResponseTo { get; private set; }

        public DateTimeOffset IssueInstant { get; private set; }

        public string Version { get; set; }

        public string Issuer { get; set; }

        public string StatusCodeValue { get; set; }

        public string AssertionId { get; set; }

        public DateTimeOffset AssertionIssueInstant { get; private set; }

        public string AssertionVersion { get; set; }

        public string AssertionIssuer { get; set; }

        public string SubjectNameId { get; set; }

        public string SubjectConfirmationMethod { get; set; }

        public string SubjectConfirmationDataInResponseTo { get; set; }

        public DateTimeOffset SubjectConfirmationDataNotOnOrAfter { get; set; }

        public string SubjectConfirmationDataRecipient { get; set; }

        public DateTimeOffset ConditionsNotBefore { get; set; }

        public DateTimeOffset ConditionsNotOnOrAfter { get; set; }

        public string Audience { get; set; }

        public DateTimeOffset AuthnStatementAuthnInstant { get; set; }

        public string AuthnStatementSessionIndex { get; set; }

        public Dictionary<string, string> SpidUserInfo { get; private set; }

        public IdpSaml2Response(
            string destination, string id, string inResponseTo, DateTimeOffset issueInstant, string version, string issuer,
            string statusCodeValue,
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
