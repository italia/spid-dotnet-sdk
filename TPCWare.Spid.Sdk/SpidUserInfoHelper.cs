using System;
using System.Collections.Generic;
using System.Text;

namespace TPCWare.Spid.Sdk
{
    public static class SpidUserInfoHelper
    {
        public static string FullName(Dictionary<string, string> spidUserInfo)
        {
            return $"{Name(spidUserInfo)} {FamilyName(spidUserInfo)}";
        }

        public static string Name(Dictionary<string, string> spidUserInfo)
        {
            return spidUserInfo["name"];
        }

        public static string FamilyName(Dictionary<string, string> spidUserInfo)
        {
            return spidUserInfo["familyName"];
        }
    }
}
