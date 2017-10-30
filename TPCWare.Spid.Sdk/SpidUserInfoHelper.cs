using System;
using System.Collections.Generic;
using System.Text;

namespace TPCWare.Spid.Sdk
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
