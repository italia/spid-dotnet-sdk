/*
  Copyright (c) 

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini - Luca Congiu (see Git history for other contributors)
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Italia.Spid.Authentication.IdP
{
    public static class IdentityProvidersList
    {
        public static List<IdentityProvider> IdpList { get; private set; }


        public static async Task<bool> LoadFromUrlAsync(string identityProviderListUrl)
        {
            bool result=false;
            using (var client = new System.Net.Http.HttpClient())
            {

                using (var response = await client.GetAsync(identityProviderListUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string json= await response.Content.ReadAsStringAsync();

                        IdpList= JsonConvert.DeserializeObject<List<IdentityProvider>>(json);
                        result = IdpList.Count > 0;

                    }

                }
            }
            return result;

        }

        public static async Task<bool> LoadFromFileAsync(string filePath)
        {
          

            if (!File.Exists(filePath)){
                throw new ArgumentNullException("Invalid FileName");
            }
           
            using (var reader = File.OpenText(filePath))
            {
               var  json = await reader.ReadToEndAsync();
                IdpList = JsonConvert.DeserializeObject<List<IdentityProvider>>(json);
            }

          
            return IdpList.Count > 0;
       
    
        }


        public static IdentityProvider GetIdpFromIdPName(string idpName)
        {
            if (string.IsNullOrWhiteSpace(idpName))
            {
                throw new ArgumentNullException("The idpName parameter can't be null.");
            }

            IdentityProvider idp = IdpList?.FirstOrDefault(x => x.IdentityProviderId == idpName);

            if (idp == null)
            {
                throw new Exception($"Error on GetIdpFromUserChoice: Identity Provider {idpName} not found.");
            }

            if (idp.IdentityProviderType==IdentityProviderType.Saml &&  string.IsNullOrWhiteSpace(idp.Settings[SamlIdentityProviderSettings.SingleSignOnServiceUrl]))
            {
                throw new Exception($"Error on GetIdpFromUserChoice: Identity Provider {idpName} doesn't have a login endpoint.");
            }

            if (idp.IdentityProviderType == IdentityProviderType.Saml && string.IsNullOrWhiteSpace(idp.Settings[SamlIdentityProviderSettings.SingleLogoutServiceUrl]))
            {
                throw new Exception($"Error on GetIdpFromUserChoice: Identity Provider {idpName} doesn't have a logout endpoint.");
            }

            return idp;
        }

    }
}
