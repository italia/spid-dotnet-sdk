using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TPCWare.Spid.Sdk.Schema;

namespace TCPWare.Spid.ChatBot.Model
{
    public class ResponseSPID
    {
        public string result { get; set; }

        public SPIDMetadata data { get; set; }
    }
}