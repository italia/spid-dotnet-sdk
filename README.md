# spid-dot-net

## Origini
Questo progetto tra origine dal progetto TPCWare SPID, frutto dell'[hackaton day](https://hack.developers.italia.it/) che si è tenuto in diverse sedi italiane il 7 e 8 ottobre 2017, realizzato da Nicolò Carandini e Antimo Musone.
Per maggiori informazioni vedi il [README](https://github.com/mrcarbook/spid-dotnet-sdk/blob/sdk-core/README.md) del progetto.

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

Nelle more della messa in produzione del Registro SPID da parte di AgID, al momento le due applicazioni web dimostrative utilizzano il meccanismo di configurazione per la creazione dell'elenco degli IdP.

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

Per quanto riguarda la Web app abbiamo scritto un codice il più pulito e commentato possibile (ma si puo sempre migliorare...) in modo da farla diventare una reference app che documenti in modo pratico l'uso dell'SDK.
Inoltre è stata aggiunta la possibilità di richiedere il logout, una volta che si è stati autenticati.

Per un tour visivo del funzionamento dell'app si rimanda alla [documentazione](https://github.com/mrcarbook/spid-dotnet-sdk/blob/sdk-core/Docs/Web%20App%20(Classic)/Documentazione%20Wep%20App.pdf)

# Sviluppi futuri

- Modifica delle applicazioni web per la generazione automatica del bottone SPID (ad oggi il contenuto del pull down menu è codificato direttamente nel codice della pagina)-

- Creazione di un componente middleware per ASP.NET Core 2.0 che incapsuli le funzionalità di autenticazione e logout della libreria in modo da rendere la libreria pluggabile e pienamente integrata nell'authorization flow delle applicazioni Web ASP.NET Core 2.0.

- Creazione di un progetto Xamarin, con applicazione dimostrativa per Android e iOS.

- Utilizzo del servizio locale di test hostato su Docker container Linux.

- Test dell'applicazione Web ASP.NET Core 2.0 su Docker container Linux.

# Build and Test

Come prima cosa occorre creare il certificato digitale self-signed, tramite [OpenSSL](https://slproweb.com/products/Win32OpenSSL.html):

Una volta installato il tool, aprire da cmd.exe la cartella C:\OpenSSL-Win64\  e lanciare i seguenti comandi per generare il certificato mycerthackdevelopers.pfx:

	set OPENSSL_CONF=C:\OpenSSL-Win64\bin\openssl.cfg
	 
	openssl req -newkey rsa:2048 -nodes -keyout myspidprivatekey.pem -x509 -days 365 -out myspidcertificate.pem
	 
	openssl pkcs12 -export -in myspidcertificate.pem -inkey myspidprivatekey.pem  -out myspidcertificate.pfx

Ora occorre importare il certificato myspidcertificate.pfx in local machine/My e tramite lo strumento di gestione dei certificati della macchina (START + "Gestisci i certificati computer") esportare il file .CER in formato BASE64, facendo attenzione di non esportare la private key.

Se l'applicazione Web da errore la causa più probabile è che il certificato dev'essere reinstallato, perché dopo qualche giorno, se pur presente, il certificato scade e dev'essere reinstallato con la medesima procedura di cui sopra.

La chiave così esportata è un mero file di testo che contiene la chiave pubblica da copiare nell'apposito campo della [pagina web di creazione del file di metadati](https://backoffice-spidtest.apps.justcodeon.it/).

Il file XML contenente i metadati creato dalla suddetta pagina va poi utilizzato per registrare la nostra applicazione Web come SP (Service Provider) presso ogni IdP che vogliamo rendere disponibile ai nostri potenziali utenti.

Ad oggi, oltre al servizio locale di test, l'unico servizio esterno di test è fornito da Poste.it ed è a loro che va inviato il suddetto file di metadati per accreditarsi come SP e fare poi le prove di autenticazione e logout.

Nota: Poiché la libreria è di tipo .NET Standard 2.0 non è possibile aprire la soluzione con versioni di Visual Studio antecedenti a [Visual Studio 2017](https://www.visualstudio.com/it/downloads/).
