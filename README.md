# Introduzione
TPCWare SPID è un progetto frutto dell'[hackaton day](https://hack.developers.italia.it/) che si è tenuto in diverse sedi italiane il 7 e 8 ottobre 2017, realizzato da Nicolò Carandini e Antimo Musone.

## 1° iterazione (Fast Rabbit: 2 gg)
Durante l'hackathon è stata realizzata una prima versione dell'SDK e di una applicazione di prova scritta in ASP.NET MVC.


Grazie al contributo del Team di supporto di Poste.it siamo riusciti nell'intento e abbiamo raggiunto tutti gli obiettivi del task dell'hackathon:

- Iscrizione come Service Provider al servizio di Identity Provider di test fornito da Poste.it tramite creazione e invio del file di metadati con i nostri dati di SP.

- Creazione e invio al suddetto servizio IdP di una richiesta di autenticazione, firmata con certificato X.509

- Ricezione della risposta dell'IdP ed estrazione dei dati contenuti nella risposta, secondo le specifiche SAML 2.

Alla fine dell'hackathon, il nostro progetto è risultato il più votato della sede di Roma.

## 2° iterazione (Wise Turtle: circa 30 gg)

Nei giorni successivi abbiamo rifattorizzato praticamente tutto, ridisegnando in primis la struttura e le funzioni dell'SDK, riscrivendo tutta una parte che inizialmente era stata tenuta fuori per problemi di compatibilità con .NET Standard. Inoltre è stata modificata la Web App dimostrativa ed è stato creato un ChatBot che si interfaccia alla Web App come uteriore esempio di utilizzo dell'SDK.

### SDK

Ora tutte le funzionalitàrelative a SAML 2.0 e al protocollo di comunicazione tra SP e IdP è interamente contenuta nell'SDK, che fornisce i seguenti servizi:

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

L'SDK contiene inoltre una serie di Helpers, utili alle applicazioni utilizzatrici dell'SDK stesso:

- Elenco degli Identity Providers, con le specifiche informazioni di endpoints (Authn e Logout) e di generazione dei formati di data (NotBefore, ecc.)

- Helper per il reperimento di certificati X509 dallo contenitore sicuro fornito dal Sistema Operativo.

- Helper per la crittografia delle richieste da inviare e per la verifica crittografica delle risposte ricevute.

- Piccolo helper per i riperimento di dati comuni dall'istanza di classe DTO che contiene i dati della risposta di authenticazione (Fullname, Name, Family name, FiscalNumber ed Email)

#### Packaging

L'SDK è stato scritto in modo da essere totalmente compatibile con .NET Standard 2.0 e con .NET 4.6.1, quindi è utilizzabile da quasiasi applicazione ASP.NET classic, ASP.NET Core (su Windows, Linux e OSX) e Xamarin (per App Android, iOS e UWP).

### Web App

Per quanto riguarda la Web app abbiamo scritto un codice il più pulito e commentato possibile (ma si puo sempre migliorare...) in modo da farla diventare una reference app che documenti in modo pratico l'uso dell'SDK.
Inoltre è stata aggiunta la possibilità di richiedere il logout, una volta che si è stati autenticati.

Per un tour visivo del funzionamento dell'app si rimanda alla documentazione:

### ChatBot

Il  progetto di Chatbot della soluzione SPID SDK è stato sviluppato utilizzando il tool open source di Microsoft, Bot Framework ( https://dev.botframework.com/ ). Attraverso il ChatBot è possibile effettuare il login SPID usando l'ambiente e l'account di test messo a disposizione da Poste italiane, sfruttando un meccanismo custom di web login basato sul progetto Web App.

Maggiori informazioni nella documentazione: 

# Prossimi passi

Nei prossimi giorni verrà completato lo sviluppo del progetto Xamarin, con applicazione dimostrativa per Android e iOS.
Per quanto riguarda l'SDK, verrà pacchettizato e pubblicato come NuGet package in modo da semplificare al massimo il suo utilizzo in progetti di altre parti.
Altro fronte che intendiamo sviluppare a breve è quello relativo all'utilizzo, sia in ambiente ASP.NET Classic che ASP.NET Core, dell'SDK in modalità IoC, in modo da renderne trasparente l'uso in applicazioni che utilizzano le modalità predefinite di autenticazione degli utenti.

# Getting Started
Come prima cosa occorre creare il certificato digitale self-signed, tramite [OpenSSL](https://slproweb.com/products/Win32OpenSSL.html):

Una volta installato il tool, aprire da cmd.exe la cartella C:\OpenSSL-Win64\  e lanciare i seguenti comandi per generare il certificato mycerthackdevelopers.pfx:

	set OPENSSL_CONF=C:\OpenSSL-Win64\bin\openssl.cfg
	 
	openssl req -newkey rsa:2048 -nodes -keyout hackdevelopersprivatekey.pem -x509 -days 365 -out hackdeveloperscertificate.pem
	 
	openssl pkcs12 -export -in hackdeveloperscertificate.pem -inkey hackdevelopersprivatekey.pem  -out mycerthackdevelopers.pfx

Ora occorre importare il certificato mycerthackdevelopers.pfx in local machine/My e tramite lo strumento di gestione dei certificati della macchina (START + "Gestisci i certificati computer") esportare il file .CER in formato BASE64, facendo attenzione di non esportare la private key.

La chiave così esportata è un mero file di testo che contiene la chiave pubblica da copiare nell'apposito campo della [pagina web di creazione del file di metadati](https://backoffice-spidtest.apps.justcodeon.it/).


# Build and Test
La nostra Hackathon Pull Request contiene una solution Visual Studio 2017 con i seguenti progetti:

- TPCWare.SPTest.SAML (libreria: testata e funzionante)
- TPCWare.SPTest.WebApp (web app: testata e funzionante)
- TPCWare.SPTest.SAML2 (libreria .NET Standard: compila, ma non testata)
- TPCWare.SPTest.AspNetCore.WebApp (web app: non ultimata)

Nota: Visual Studio 2017 deve essere eseguito con i privilegi di Amministratore, altrimenti non è possibile accedere al certificato self-signed.
