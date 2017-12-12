/*
  Copyright (c) 2017 TPCWare - Nicolò Carandini

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini (see Git history for other contributors)
*/

using System;

namespace Italia.Spid.Authentication.IdP
{
    public class IdpLogoutResponse
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

        public bool IsSuccessful
        {
            get { return StatusCodeValue == "Success"; }
        }

        public IdpLogoutResponse(string destination, string id, string inResponseTo, DateTimeOffset issueInstant, string version, string issuer,
                                 string statusCodeValue, string statusCodeInnerValue, string statusMessage, string statusDetail)
        {
            Destination = destination;
            Id = id;
            InResponseTo = inResponseTo;
            IssueInstant = issueInstant;
            Version = version;
            Issuer = issuer;
            StatusCodeValue = statusCodeValue;
            StatusCodeInnerValue = statusCodeInnerValue;
            StatusMessage = statusMessage;
            StatusDetail = statusDetail;
        }
    }
}
