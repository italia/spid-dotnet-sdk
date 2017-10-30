using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Microsoft.Bot.Builder.FormFlow;
using Newtonsoft.Json;
using TCPWare.Spid.ChatBot.Model;

namespace TCPWare.Spid.ChatBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public const string UrlSPIDIntegrationService = "http://localhost:62371";

        public const string AccountStateKey = "AccountState";
        public const string OptionLogin = "SPID Login";
        public const string OptionSupporto = "Supporto";
        public const string LastVisitKey = "LastVisit";
        private const string retry = "Non è una valida opzione";
        private int attempts = 3;
        bool isRequestOK = false;
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {

            //first time
            if (context.UserData.GetValueOrDefault<AccountState>(AccountStateKey) == AccountState.Unknown)
            {
                context.UserData.SetValue(LastVisitKey, "");

                ThumbnailCard card = new ThumbnailCard()
                {
                    Title = "",
                    Text = "SPID è un servizio  che ti permette di accedere con il tuo profilo ovunque!",
                    Images = (IList<CardImage>)new List<CardImage>()
                  {
                       new CardImage("https://login.regione.umbria.it/wayf/images/spid-agid-logo-lb.png", (string) null, (CardAction) null)

                  }
                };
                IMessageActivity message = context.MakeMessage();
                message.Attachments.Add(card.ToAttachment());
                await context.PostAsync(message, new System.Threading.CancellationToken());

                context.UserData.SetValue<AccountState>(AccountStateKey, AccountState.Init);


            }
            else
                context.UserData.SetValue(LastVisitKey, DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year);


            context.Wait(ManageMenuAsync);
        }

        public virtual async Task ManageMenuAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            ShowNoLoginMenuOptions(context);
        }

        private void ShowNoLoginMenuOptions(IDialogContext context)
        {
            List<string> optionList = new List<string>();

            optionList.Add(OptionSupporto);
            optionList.Add(OptionLogin);


            var spid = context.UserData.GetValueOrDefault<string>("SPID");

            string prompt = string.Format("Ciao {0},in cosa posso esserti utile ?", context.Activity.From.Name != null ? context.Activity.From.Name != null ? context.Activity.From.Name.ToString() : "" : "");

            PromptDialog.Choice(context, this.OnOptionSelected, (IEnumerable<string>)optionList, prompt, retry, attempts);
        }

        public virtual async Task ManageRetryMenuAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            ShowRetryLoginMenuOptions(context);
        }



        private void ShowRetryLoginMenuOptions(IDialogContext context)
        {
            List<string> optionList = new List<string>();

            optionList.Add(OptionSupporto);
            optionList.Add(OptionLogin);


            var spid = context.UserData.GetValueOrDefault<string>("SPID");

            string prompt = string.Format("In cosa posso esserti utile ?", context.Activity.From.Name != null ? context.Activity.From.Name != null ? context.Activity.From.Name.ToString() : "" : "");

            PromptDialog.Choice(context, this.OnOptionSelected, (IEnumerable<string>)optionList, prompt, retry, attempts);
        }

        public virtual async Task VerificaSPIDMenuAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            List<string> optionList = new List<string>();
            optionList.Add("Verifica Login SPID");
            optionList.Add(OptionSupporto);

            var spid = context.UserData.GetValueOrDefault<string>("SPID");

            string prompt = string.Format("Sei hai effettuato il login con SPID e tutto è andato a buon fine premi il tasto di verifica!");

            PromptDialog.Choice(context, this.OnOptionSelected, (IEnumerable<string>)optionList, prompt, retry, attempts);
        }


        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case OptionSupporto:
                        await context.PostAsync("Per domande e supporto chiedi a Nicolò o Antimo! Grazie!");
                        break;
                    case OptionLogin:
                        var spidFormDialog = FormDialog.FromForm(this.BuilSpidForm, FormOptions.PromptInStart);

                        context.Call(spidFormDialog, this.ResumeAfterSPIDFormDialog);

                        break;
                    case "Verifica Login SPID":

                        var cf = context.UserData.GetValueOrDefault<string>("CF");

                        //Build the URI
                        Uri checkLoginSPIDUri = new Uri(UrlSPIDIntegrationService + "/Home/checkspidlogin?cf=" + cf);

                        //Send the POST request
                        using (System.Net.WebClient client = new System.Net.WebClient())
                        {
                            //Set the encoding to UTF8
                            client.Encoding = System.Text.Encoding.UTF8;

                            string responseString = client.DownloadString(checkLoginSPIDUri.AbsoluteUri);


                            ResponseSPID myData = JsonConvert.DeserializeObject<ResponseSPID>(responseString);

                            if (myData.result != "false")
                                await context.PostAsync("Ecco i tuoi dati, Nome: " + myData.data.Name + ", Cognome: " + myData.data.FamilyName + ", Email: " + myData.data.Email);
                            else

                                await context.PostAsync("Riconoscimento fallito");

                        }


                        break;


                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Troppi tentaivi errati, riprova!");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private IForm<SPIDForm> BuilSpidForm()
        {
            OnCompletionAsyncDelegate<SPIDForm> processRequest = async (context, form) =>
            {
                await context.PostAsync($"Elaborazione in corso....");

                if (!String.IsNullOrEmpty(form.CF))
                {
                    isRequestOK = true;

                    context.UserData.SetValue<string>("CF", form.CF);
                }
            };

            var formToBuild = new FormBuilder<SPIDForm>();

            formToBuild.Field(nameof(SPIDForm.CF));

            formToBuild.OnCompletion(processRequest);

            return formToBuild.Build();

        }

        private async Task ResumeAfterSPIDFormDialog(IDialogContext context, IAwaitable<SPIDForm> result)
        {
            try
            {
                var searchQuery = await result;

                if (isRequestOK)
                {
                    var resultMessage = context.MakeMessage();

                    resultMessage.Attachments = new List<Attachment>();


                    HeroCard heroCard = new HeroCard()
                    {
                        Title = "Login SPID",
                        Subtitle = $"Scegli uno dei seguenti provider di SPID! Una volta effettuato il login clicca sul pulsante Fatto",
                        Images = new List<CardImage>()
                    {
                        new CardImage() { Url = "http://servizi.comune.fi.it/sites/www.comune.fi.it/files/spid-immagine-sistema-identita-digitale.jpg" }
                    },
                        Buttons = new List<CardAction>()
                    {
                                 new CardAction()
                        {
                            Title = "Poste ID",
                            Type = ActionTypes.OpenUrl,
                            Value = $"{UrlSPIDIntegrationService}/Home/SpidRequest?idpLabel=poste_id"
                        },
                        new CardAction()
                        {
                            Title = "InfoCert",
                            Type = ActionTypes.OpenUrl,
                              Value = $"{UrlSPIDIntegrationService}/Home/SpidRequest?idpLabel=infocert_id"
                        },
                         new CardAction()
                        {
                            Title = "Aruba",
                            Type = ActionTypes.OpenUrl,
                             Value = $"{UrlSPIDIntegrationService}/Home/SpidRequest?idpLabel=aruba_id"
                        }
                         ,
                         new CardAction()
                        {
                            Title = "TIM",
                            Type = ActionTypes.OpenUrl,
                               Value = $"{UrlSPIDIntegrationService}/home/SpidRequest?idRequest=tim_id"
                        },
                            new CardAction()
                        {
                            Title = "Fatto",
                            Type = ActionTypes.PostBack,

                            Value = "Verifica Login SPID"
                        }
                    }
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());

                    await context.PostAsync(resultMessage);

                    context.Wait(this.VerificaSPIDMenuAsync);
                    //context.Done<bool>(true);
                }
                else
                {
                    await context.PostAsync("Devi inserire un CF valido");

                    context.Wait(ManageMenuAsync);

                    //context.Done<bool>(false);
                }

            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "Abbiamo riscontrato un problema , l'operazione è stata annullat. ";
                }
                else
                {
                    reply = $"Oops! Qualcosa è andato storto :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);


                context.Done<bool>(false);

            }

        }
    }

    public enum AccountState
    {
        Unknown, Init, ToRegistered, Registering, Actived
    }
}