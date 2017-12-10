/*
  Copyright (c) 2017 TEAM PER LA TRASFORMAZIONE DIGITALE

  This file is licensed to you under the BSD 3-Clause License.
  See the LICENSE file in the project root for more information.

  Authors: Nicolò Carandini (see Git history for other contributors)
*/

using System.Collections.Generic;

namespace Italia.Spid.Authentication
{
    public static class SpidUserInfoHelper
    {
        public static string FullName(Dictionary<string, string> spidUserInfo)
        {
            string fullname = string.Empty;

            string name = Name(spidUserInfo);
            if (name != "N/A")
            {
                fullname = name;
            }

            string familyName = FamilyName(spidUserInfo);
            if (familyName != "N/A")
            {
                fullname += " " + familyName;
            }

            if (string.IsNullOrWhiteSpace(fullname))
            {
                fullname = "N/A";
            }

            return fullname.Trim();
        }

        public static string Name(Dictionary<string, string> spidUserInfo)
        {
            try
            {
                return spidUserInfo["name"];
            }
            catch
            {
                return "N/A";
            }
        }

        public static string FamilyName(Dictionary<string, string> spidUserInfo)
        {
            try
            {
                return spidUserInfo["familyName"];
            }
            catch
            {
                return "N/A";
            }
        }

        public static string FiscalNumber(Dictionary<string, string> spidUserInfo)
        {
            try
            {
                return spidUserInfo["fiscalNumber"];
            }
            catch
            {
                return "N/A";
            }
        }

        public static string Email(Dictionary<string, string> spidUserInfo)
        {
            try
            {
                return spidUserInfo["email"];
            }
            catch
            {
                return "N/A";
            }
        }

    }
}
