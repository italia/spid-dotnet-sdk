/*
  Copyright (c) 2017 TEAM PER LA TRASFORMAZIONE DIGITALE

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini (see Git history for other contributors)
*/

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Italia.Spid.Authentication
{
    public static class X509Helper
    {
        /// <summary>
        /// Get certificate from file path and password
        /// </summary>
        /// <param name="certFilePath"></param>
        /// <param name="certPassword"></param>
        /// <returns></returns>
        public static X509Certificate2 GetCertificateFromFile(string certFilePath, string certPassword)
        {
            if (string.IsNullOrWhiteSpace(certFilePath))
            {
                throw new ArgumentNullException("The certFilePath parameter can't be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(certPassword))
            {
                throw new ArgumentNullException("The certPassword parameter can't be null or empty.");
            }

            if (File.Exists(certFilePath))
            {
                return new X509Certificate2(certFilePath, certPassword);
            }
            else
            {
                throw new FileNotFoundException("Unable to locate certificate");
            }
        }

        /// <summary>
        /// Get certificate from the store
        /// </summary>
        /// <param name="storeLocation"></param>
        /// <param name="storeName"></param>
        /// <param name="findType"></param>
        /// <param name="findValue">Must be a string or a DateTime, depending on findType</param>
        /// <param name="validOnly">Must be false if testing with a self-signed certificate</param>
        /// <returns></returns>
        public static X509Certificate2 GetCertificateFromStore(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue, bool validOnly)
        {
            X509Certificate2 certificate = null;

            if (findValue == null)
            {
                throw new ArgumentNullException("The findValue parameter can't be null.");
            }

            try
            {
                X509Store store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection coll = store.Certificates.Find(findType, findValue.ToString(), validOnly);

                if (coll.Count > 0)
                {
                    certificate = coll[0];
                }
                store.Close();

                if (certificate != null)
                {
                    return certificate;
                }
                else
                {
                    throw new FileNotFoundException("Unable to locate certificate");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
