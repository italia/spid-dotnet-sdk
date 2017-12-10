using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Italia.Spid.AspNetCore.WebApp.Models
{
    public class UserInfo
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string FiscalNumber { get; set; }

        public string Email { get; set; }
    }
}