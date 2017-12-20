/*
  Copyright (c) 2017 TPCWare - Nicolò Carandini

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini (see Git history for other contributors)
*/

using System;

namespace Italia.Spid.Authentication.IdP
{
    public class IdentityProvider
    {
        private string subjectNameIdRemoveText;
        private string dateTimeFormat;
        private double nowDelta; // We need this to be compliant with Sielte IdP (value needs to be -2 for Sielte, 0 for all others)

        public string EntityID { get; private set; }

        public string OrganizationName { get; set; }

        public string OrganizationDisplayName { get; set; }

        public string OrganizationUrl { get; set; }

        public string SingleSignOnServiceUrl { get; private set; }

        public string SingleLogoutServiceUrl { get; private set; }


        public IdentityProvider(
            string entityId,
            string organizationName,
            string organizationDisplayName,
            string organizationUrl,
            string singleSignOnServiceUrl,
            string singleLogoutServiceUrl,
            string subjectNameIdRemoveText,
            string dateTimeFormat,
            double nowDelta)
        {
            EntityID = entityId;
            OrganizationName = organizationName;
            OrganizationDisplayName = organizationDisplayName;
            OrganizationUrl = organizationUrl;
            SingleSignOnServiceUrl = singleSignOnServiceUrl;
            SingleLogoutServiceUrl = singleLogoutServiceUrl;
            this.subjectNameIdRemoveText = subjectNameIdRemoveText;
            this.dateTimeFormat = dateTimeFormat;
            this.nowDelta = nowDelta;
        }

        public string SubjectNameIdFormatter(string s)
        {
            return (string.IsNullOrWhiteSpace(subjectNameIdRemoveText)) ? s : s.Replace(subjectNameIdRemoveText, "");
        }

        public string Now(DateTime now)
        {
            return now.AddMinutes(nowDelta).ToString(dateTimeFormat);
        }

        public string After(DateTime after)
        {
            return after.ToString(dateTimeFormat);
        }

        public string NotBefore(DateTime now)
        {
            return now.AddMinutes(-2).ToString(dateTimeFormat);
        }

        internal void ConfigOverrideSpidServiceUrl(string singleSignOnServiceUrl)
        {
            this.SingleSignOnServiceUrl = singleSignOnServiceUrl;
        }

        internal void ConfigOverrideLogoutServiceUrl(string singleLogoutServiceUrl)
        {
            this.SingleLogoutServiceUrl = singleLogoutServiceUrl;
        }

        internal void ConfigOverrideDateTimeFormat(string dateTimeFormat)
        {
            this.dateTimeFormat = dateTimeFormat;
        }

        internal void ConfigOverrideNowDelta(double nowDelta)
        {
            this.nowDelta = nowDelta;
        }
    }
}
