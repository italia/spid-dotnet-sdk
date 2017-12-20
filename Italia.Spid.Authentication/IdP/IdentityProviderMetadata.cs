/*
  Copyright (c) 2017 TPCWare - Nicolò Carandini

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini (see Git history for other contributors)
*/

namespace Italia.Spid.Authentication.IdP
{
    public class IdentityProviderMetaData
    {
        public string EntityId { get; set; }

        public string OrganizationName { get; set; }

        public string OrganizationDisplayName { get; set; }

        public string OrganizationUrl { get; set; }

        public string SingleSignOnServiceUrl { get; set; }

        public string SingleLogoutServiceUrl { get; set; }
    }
}