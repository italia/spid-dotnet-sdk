⚠️ Questo repository è deprecato e sconsigliato è il uso in produzione. Si consiglia l'uso di altre SDK SPID disponibili su Developetrs Italia.


# spid-dot-net

## Origini
Questo progetto tra origine dal progetto TPCWare SPID, frutto dell'[hackaton day](https://hack.developers.italia.it/) che si è tenuto in diverse sedi italiane il 7 e 8 ottobre 2017, realizzato da Nicolò Carandini e Antimo Musone.
Maggiori informazioni possono essere trovate nel [README](https://github.com/mrcarbook/spid-dotnet-sdk/blob/sdk-core/README.md) del progetto originario.

## Contenuto
Il repository contiene i seguenti progetti:

- Libreria di autenticazione SPID (protocollo SAML2)
- Applicazione dimostrativa ASP.NET
- Applicazione dimostratima ASP.NET Core 

### Libreria di autenticazione

La libreria implementa tutte le funzionalità relative a SAML 2.0 e al protocollo di comunicazione tra SP e IdP tramite i seguenti servizi:

#### Creazione e configurazione dell'elenco degli IdP

La libreria offre servizi orientati alla creazione dell'elenco degli IdP (Identity Providers) a partire dall'URL del servizio di metadati forniti dall'AgID (Agenzia per l'Italia Digitale) secondo quanto previsto dalle [norme tecniche SPID](https://www.spid.gov.it/assets/res/AgID-SPID-InfoSP.pdf).

Tale elenco può essere ampliato e arricchito con ulteriori dettagli dall'applicazione che usa la libreria di autenticazione, tipicamente tramite file di configurazione.

Al momento le due applicazioni web dimostrative utilizzano il meccanismo di configurazione per la creazione dell'elenco degli IdP, ma è in corso di sviluppo la funzionalità di lettura dell'elenco IdP dal [registro SPID fornito da AgID](https://registry.spid.gov.it/identity-providers).

#### Richiesta Authn

1. Creazione della richiesta di Authn in formato SAML 2.0
2. Firma della richiesta con certificato X.509
3. Analisi di validità e di stato della risposta dell'IdP relativa alla richiesta di Authn
4. Estrazione dei dati dal formato XAML 2.0 ad una classe DTO tipizzata, contenente tutte le informazioni ricevute dall'IdP

#### Richiesta Logout

1. Creazione della richiesta di Logout in formato SAML 2.0
2. Firma della richiesta con certificato X.509
3. Analisi di validità e di stato della risposta dell'IdP relativa alla richiesta di Logout
4. Estrazione dei dati dal formato XAML 2.0 ad una classe DTO tipizzata, contenente tutte le informazioni ricevute dall'IdP

#### Helpers

La libreria contiene inoltre una serie di Helpers, utili alle applicazioni che la utilizzano:

- Helper per il reperimento di certificati X509 dal contenitore sicuro (disponibile solo quando l'applicazione web gira su Windows, negli altri casi è possibile passare alla libreria direttamente il certificato stesso).

- Helper per la crittografia delle richieste da inviare e per la verifica crittografica delle risposte ricevute.

- Piccolo helper per i riperimento di dati comuni dall'istanza di classe DTO che contiene i dati della risposta di authenticazione (Fullname, Name, Family name, FiscalNumber ed Email)

#### Packaging

La libreria di autenticazione è stata scritta in modo da essere totalmente compatibile con .NET Standard 2.0 e con .NET 4.6.1, quindi è utilizzabile da quasiasi applicazione ASP.NET classic, ASP.NET Core (su Windows, Linux e OSX) e Xamarin (per App Android, iOS e UWP).

Per l'uso in produzione, si consiglia di utilizzare la libreria aggiungendo al progetto il pacchetto NuGet `Italia.SPid.Authentication`.

### Web App

Per quanto riguarda le Web app (ASP.NET e ASP.NET Core 2.0) abbiamo cercato di scrivere un codice il più pulito e commentato possibile (ma si puo sempre migliorare...) in modo da farla diventare una reference app che documenti in modo pratico l'uso della libreria di autenticazione.

Per un tour visivo del funzionamento dell'app si rimanda alla [documentazione](https://github.com/ncarandini/spid-dotnet-sdk/blob/master/Docs/Web%20App%20(Classic)/Documentazione%20Wep%20App.pdf)

# Sviluppi futuri

- Modifica delle applicazioni web per la generazione automatica del bottone SPID (ad oggi il contenuto del pull down menu è codificato direttamente nel codice della pagina)-

- Creazione di un componente middleware per ASP.NET Core 2.0 che incapsuli le funzionalità di autenticazione e logout della libreria in modo da rendere la libreria pluggabile e pienamente integrata nell'authorization flow delle applicazioni Web ASP.NET Core 2.0.

- Creazione di un progetto Xamarin, con applicazione dimostrativa per Android e iOS.

- Utilizzo del servizio locale di test hostato su Docker container Linux.

- Test dell'applicazione Web ASP.NET Core 2.0 su Docker container Linux.

- Scrittura della documentazione, al momento (davvero troppo) minimale.

# Build and Test

Come prima cosa occorre creare il certificato digitale self-signed, tramite [OpenSSL](https://slproweb.com/products/Win32OpenSSL.html):

Una volta installato il tool, aprire da cmd.exe la cartella C:\OpenSSL-Win64\  e lanciare i seguenti comandi per generare il certificato mycerthackdevelopers.pfx:

	set OPENSSL_CONF=C:\OpenSSL-Win64\bin\openssl.cfg
	 
	openssl req -newkey rsa:2048 -nodes -keyout myspidprivatekey.pem -x509 -days 365 -out myspidcertificate.pem
	 
	openssl pkcs12 -export -in myspidcertificate.pem -inkey myspidprivatekey.pem  -out myspidcertificate.pfx

Ora occorre importare il certificato myspidcertificate.pfx in local machine/My e tramite lo strumento di gestione dei certificati della macchina (START + "Gestisci i certificati computer") esportare il file .CER in formato BASE64, facendo attenzione di non esportare la private key.

Se l'applicazione Web da errore "Unable to find private key in the X509Certificate" la causa più probabile è che il certificato non è più valido e deve essere reinstallato con la medesima procedura di cui sopra.

Il file così ottenuto contiene la chiave pubblica da utilizzare per la creazione del file XML dei metadati da inviare all'AgID (Agenzia per l'Italia Digitale) secondo le modalità di accreditamento descritte nella pagina [Come diventare fornitore di servizi Pubblici o Privati con SPID](https://www.spid.gov.it/come-diventare-fornitore-di-servizi-pubblici-e-privati-con-spid).

Una volta accreditati con AgID, il suddetto file di metadati verrà inviato direttamenta da AgID ai vari IdP e si potrà testare il servizio e mandarlo in produzione.

Ad oggi, oltre al servizio locale di test, l'unico servizio esterno di test è fornito da Poste.it ed è a loro che va inviato il suddetto file di metadati per accreditarsi come SP e fare poi le prove di autenticazione e logout.

Nota: Poiché la libreria è di tipo .NET Standard 2.0 non è possibile aprire la soluzione con versioni di Visual Studio antecedenti a [Visual Studio 2017](https://www.visualstudio.com/it/downloads/).
