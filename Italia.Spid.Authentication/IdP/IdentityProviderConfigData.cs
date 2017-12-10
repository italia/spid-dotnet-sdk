/*
  Copyright (c) 2017 TEAM PER LA TRASFORMAZIONE DIGITALE

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini (see Git history for other contributors)
*/

namespace Italia.Spid.Authentication.IdP
{
    public class IdentityProviderConfigData: IdentityProviderMetaData
    {
        public string SubjectNameIdRemoveText { get; set; }

        public string DateTimeFormat { get; set; }

        public double NowDelta { get; set; }
    }
}