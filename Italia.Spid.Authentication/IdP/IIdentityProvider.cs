/*
  Copyright (c) 2017 TPCWare - Nicolò Carandini

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini (see Git history for other contributors)
*/

using System;
using System.Collections.Generic;

namespace Italia.Spid.Authentication.IdP
{
    public interface IIdentityProvider
    {

        string IdentityProviderId { get; set; }

        string OrganizationName { get; set; }

        string OrganizationDisplayName { get; set; }

        string OrganizationUrl { get; set; }

        string OrganizationLogoUrl { get; set; }

        //
        IdentityProviderType IdentityProviderType { get; }
        Dictionary<string, string> Settings { get; set; }
        //



    }


}
