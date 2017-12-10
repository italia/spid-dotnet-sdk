/*
  Copyright (c) 2017 TEAM PER LA TRASFORMAZIONE DIGITALE

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini (see Git history for other contributors)
*/

using System;
using System.Collections.Generic;

namespace Italia.Spid.Authentication.Schema
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