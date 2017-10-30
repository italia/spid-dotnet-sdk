using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCPWare.Spid.ChatBot.Dialogs
{
    [Serializable]
    public class SPIDForm
    {
         
        [Prompt("Per cortesia inserisci il tuo codice fiscale")]
        public string CF { get; set; }

    }
}