# Introduction 
TPCWare SPID è un progetto frutto dell'[hackaton day](https://hack.developers.italia.it/) che si è tenuto in diverse sedi italiane il 7 e 8 ottobre 2017, realizzato da Nicolò Carandini e Antimo Musone.

Si tratta di un lavoro iniziale di creazione dell'SDK per .NET di SPID, che al momento consiste in due librerie:

- TPCWare.Spid.SAML (una lib .NET Framework 4.6.1)
- TPCWare.Spid.SAML2 (una lib .NET Standard 2.0)

Entrambe basate su SAML 2.0, la prima è quella che abbiamo  utilizzato nella Web App ASP.NET (Classic) mentre la seconda è quella che abbiamo realizzato per poterla utilizzare su progetti Xamarin.Forms e ASP.NET Core 2.0.

# Cose da fare
L'attuale implementazione di Visual Studio for Mac non ci consente (ancora) di gestire i progetti Xamarin.Forms e nel corso di questo hackaton, avendo speso molto tempo sia sul fronte del certificato self-signed e sulla generazione del relativo file di metadati da inviare a poste.it che sul fronte Xamarin, non abbiamo potuto completare la web app ASP.NET Core.

Entrambi questi task verranno effettuati nei successivi 30 giorni.

Il fatto di aver completato il porting della libreria in formato .NET Standard consente l'utilizzo dell'SDK anche in scenari Linux, grazie all'uso di ASP.NET Core.
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
