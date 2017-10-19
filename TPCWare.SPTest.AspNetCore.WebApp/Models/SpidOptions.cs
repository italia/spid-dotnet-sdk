using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TPCWare.SPTest.AspNetCore.WebApp.Models
{
    public class SpidOptions
    {
        public string CertificateName { get; set; } = "HackDevelopers";

        public string DomainValue { get; set; } = "https://www.tpcware.com";

        public string CookieId { get; set; } = "tcpwarespid";
    }
}
