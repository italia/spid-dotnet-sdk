/*
  Copyright (c) 2017 TEAM PER LA TRASFORMAZIONE DIGITALE

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini (see Git history for other contributors)
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Italia.Spid.Authentication.IdP
{
    public static class IdentityProvidersList
    {
        public static List<IdentityProvider> IdpList { get; private set; }

        public static void IdentityProvidersListFactory(List<IdentityProviderConfigData> idpConfigDataList)
        {
            IdentityProvidersListFactory(null, idpConfigDataList);
        }

        public static void IdentityProvidersListFactory(List<IdentityProviderMetaData> ipdMetaDataList, List<IdentityProviderConfigData> idpConfigDataList = null)
        {
            IdpList = new List<IdentityProvider>();

            // Create IdPConfigData list from metadata and standard values
            if (ipdMetaDataList?.Count > 0)
            {
                foreach (var ipdMetaData in ipdMetaDataList)
                {
                    IdpList.Add(new IdentityProvider(
                        providerName: ipdMetaData.ProviderName,
                        spidServiceUrl: ipdMetaData.SpidServiceUrl,
                        logoutServiceUrl: ipdMetaData.LogoutServiceUrl,
                        subjectNameIdRemoveText: string.Empty,
                        dateTimeFormat: "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'",
                        nowDelta: 0
                    ));
                }
            }

            // Add IdPs from config file, eventually overriding standard values
            if (idpConfigDataList.Count > 0)
            {
                foreach (var idpConfigData in idpConfigDataList)
                {
                    if (string.IsNullOrWhiteSpace(idpConfigData.ProviderName))
                    {
                        throw new ArgumentNullException("The ProviderName property of a idpConfigData (Identity Provider configuration data) item can't be null.");
                    }

                    var foundElement = IdpList.FirstOrDefault(x => x.ProviderName == idpConfigData.ProviderName);

                    if (foundElement != null)
                    {
                        // Override SpidServiceUrl with config data, if present
                        if (!string.IsNullOrWhiteSpace(idpConfigData.SpidServiceUrl))
                        {
                            foundElement.ConfigOverrideSpidServiceUrl(idpConfigData.SpidServiceUrl);
                        }
                        // Override LogoutServiceUrl with config data, if present
                        if (!string.IsNullOrWhiteSpace(idpConfigData.LogoutServiceUrl))
                        {
                            foundElement.ConfigOverrideLogoutServiceUrl(idpConfigData.LogoutServiceUrl);
                        }
                        // Override DateTimeFormat with config data, if present
                        if (!string.IsNullOrWhiteSpace(idpConfigData.DateTimeFormat))
                        {
                            foundElement.ConfigOverrideDateTimeFormat(idpConfigData.DateTimeFormat);
                        }
                        // Override NowDelta with config data, if present
                        foundElement.ConfigOverrideNowDelta(idpConfigData.NowDelta);
                    }
                    else
                    {
                        // Check config data consistency
                        if (string.IsNullOrWhiteSpace(idpConfigData.SpidServiceUrl))
                        {
                            throw new ArgumentNullException("When adding a IdP from Config, the SpidServiceUrl property can't be null or empty.");
                        }
                        if (string.IsNullOrWhiteSpace(idpConfigData.LogoutServiceUrl))
                        {
                            throw new ArgumentNullException("When adding a IdP from Config, the LogoutServiceUrl property can't be null or empty.");
                        }

                        // Set config details from config data or default values
                        string subjectNameIdRemoveText = string.IsNullOrWhiteSpace(idpConfigData.SubjectNameIdRemoveText) ? string.Empty : idpConfigData.SubjectNameIdRemoveText;
                        string nowFormatText = string.IsNullOrWhiteSpace(idpConfigData.DateTimeFormat) ? "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'" : idpConfigData.DateTimeFormat;

                        // Add IdP from config data
                        IdpList.Add(new IdentityProvider(
                            providerName: idpConfigData.ProviderName,
                            spidServiceUrl: idpConfigData.SpidServiceUrl,
                            logoutServiceUrl: idpConfigData.LogoutServiceUrl,
                            subjectNameIdRemoveText: subjectNameIdRemoveText,
                            dateTimeFormat: nowFormatText,
                            nowDelta: idpConfigData.NowDelta
                        ));
                    }
                }
            }
        }

        public static async Task<List<IdentityProviderMetaData>> GetIdpMetaDataListAsync(string idpMetadataListUrl)
        {
            List<IdentityProviderMetaData> ipdMetaDataList = new List<IdentityProviderMetaData>();

            // Read IdPs metadata from AGId resource and add to the list
            if (!string.IsNullOrWhiteSpace(idpMetadataListUrl))
            {
                // TODO!
            }

            return ipdMetaDataList;
        }

        public static IdentityProvider GetIdpFromIdPName(string idpName)
        {
            if (string.IsNullOrWhiteSpace(idpName))
            {
                throw new ArgumentNullException("The idpName parameter can't be null.");
            }

            IdentityProvider idp = IdpList?.FirstOrDefault(x => x.ProviderName == idpName);

            if (idp == null)
            {
                throw new Exception($"Error on GetIdpFromUserChoice: Identity Provider {idpName} not found.");
            }

            if (string.IsNullOrWhiteSpace(idp.SpidServiceUrl))
            {
                throw new Exception($"Error on GetIdpFromUserChoice: Identity Provider {idpName} doesn't have a login endpoint.");
            }

            if (string.IsNullOrWhiteSpace(idp.LogoutServiceUrl))
            {
                throw new Exception($"Error on GetIdpFromUserChoice: Identity Provider {idpName} doesn't have a logout endpoint.");
            }

            return idp;
        }

    }
}
