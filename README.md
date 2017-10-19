Developers.Italia.SPID.SAML
=

Questa libreria permette di interfacciare un'applicazione ASP.NET con un IdP (Identity Provider) abilitato a **SPID** ([Sistema Pubblico di Identità Digitale](https://www.spid.gov.it])).

Tramite questa libreria è possibile effettuare l'autenticazione di un Utente registrato presso un Identity Provider attivo e richiedere alcuni dati dell'Utente (Cittadino)

### Prerequisiti
#### Certificati per firma digitale
___

Per eseguire delle chiamate ad un Identity Provider (IdP) è necessario avere a disposizione un certificato per la firma della richiesta.

Nell'ambiente di test è possibile creare dei certificati self-signed tramite OpenSSL.

**Linux Download:**

https://www.openssl.org/source/

**Windows Download:**

https://slproweb.com/products/Win32OpenSSL.html

Per creare i certificati basta eseguire:
```shell
> Openssl req -newkey rsa:2048 -keyout certificati\spid-developersitalia.key  -x509 -days 365 -out certificati\spid-developersitalia.crt
```
Una volta generato il certificato e la chiave privata esportiamo certificato e chiave in un unico file pfx:
```shell
> Openssl pkcs12 -export -out certificati\spid-developersitalia.pfx -inkey certificati\spid-developersitalia.key -in certificati\spid-developersitalia.crt
```

Controllo Certificato:
https://www.sslshopper.com/certificate-decoder.html

#### Metadata Identity Provider (IdP)
___

Per la comunicazione verso un Identity Provider è necessario avere a disposizione i metadata che descrivono i servizi messi a disposizione dall' IdP.
Le informazioni più importanti, oltre al certificato utilizzato dall' IdP, sono:

* **SingleSignOnService** (HTTP-POST)
 
  Entry-point per l'autenticazione tramite HTTP-POST
* **SingleSignOnService** (HTTP-REDIRECT)

  Entry-point per l'autenticazione tramite HTTP-REDIRECT

* **SingleLogoutService** (HTTP-POST)

  Entry-point per il logout tramite HTTP-POST

* **SingleLogoutService** (HTTP-REDIRECT)

  Entry-point per il logout tramite HTTP-POST


Potete trovare alcuni metadata di esempio qui: https://github.com/congiuluc/spid-dotnet-sdk/tree/master/src/Developers.Italia.SPID/SAML/Metadata/IdP/




#### Metadata Service Provider (SP)
___
Per il corretto scambio di informazioni tra IdP e SP è necessario che anche il SP invii all' IdP il metadata contenente le configurazioni dell'SP.

Oltre alle informazioni del certificato che verrà utilizzato per la firma, le principali informazioni da inserire sono:

* **EntityID**

  Identificativo Univoco del Serive Provider (normalmente identificato dall'url principale)

* **AssertionConsumerService**

  Url che l'IdP deve richiamare a dopo l'autenticazione dell'utente (può essere sia HTTP-POST che HTTP-REDIRECT)
* **AttributeConsumingService**

  Elenco degli attributi che verranno restituiti al SP dall'IdP come p.es nome, cognome, codice fiscale etc

* **SingleLogoutService**

  Url che l'IdP deve richiamare a fronte di una richiesta di Logout (può essere sia HTTP-POST che HTTP-REDIRECT)


Potete trovare un metadata di esempio qui: https://github.com/congiuluc/spid-dotnet-sdk/tree/master/src/Developers.Italia.SPID/SAML/Metadata/SP/



https://github.com/italia/spid-testenv-backoffice


#### ASP.NET Core 2 FIX
___
Ad oggi, solo per le applicazioni ASP.NET Core 2, è necessario aggiungere il feed a myget del componente [System.Security.Cryptography.Xml 4.5.0](https://dotnet.myget.org/feed/dotnet-core/package/nuget/System.Security.Cryptography.Xml) questo perchè, l'attuale versione System.Security.Cryptography.Xml non è supportata in .net core mentre la versione 4.5.0 supporta .NET STANDARD 2.0.

Per aggiungere il feed basta aprire il menù delle opzioni di Visual Studio (menu Tools > Options) e selezionare NuGet Package Manager > Package Source ed aggiungere il seguente feed myGet:
>https://dotnet.myget.org/F/dotnet-core/api/v3/index.json

**N.B.:** Non appena sarà rilasciata la versione 4.5.0 non sarà più necessario aggiungere il feed.

<br/>

### Esempi di Utilizzo

#### Creazione Nuova Richiesta:

```csharp
 Developers.Italia.SPID.SAML.AuthRequestOptions options = new Developers.Italia.SPID.SAML.AuthRequestOptions()
 {
    //Request Unique Identifier
    UUID = Guid.NewGuid().ToString(),
    //Service Provider Unique Identifier (url like https://www.dotnetcode.it can be ok)
    SPUID = "MyServiceProviderUniqueIdentifier",
    //SPID Authentication Level (1/2/3)
    SPIDLevel = Developers.Italia.SPID.SAML.SPIDLevel.SPIDL1,
    //Identity Provider Url For SAML Request
    Destination = "https://www.myserviceprovider.com/samlurl",
    //Service Provider Consumer Service Index - Normally Callback URL (refer to Service Provider Metadata)
    AssertionConsumerServiceIndex = 0,
    //Service Provider Consuming Service Index - Normally Requested User Data(refer to Service Provider Metadata)
    AttributeConsumingServiceIndex = 0
 };

 Developers.Italia.SPID.SAML.AuthRequest request = new Developers.Italia.SPID.SAML.AuthRequest(options);

 string rsaXmlKey = "<![CDATA[<RSAKeyValue><Modulus>rBPwxOB3QM+Rhz+/...</RSAKeyValue>";
 
 X509Certificate2 signinCert = new X509Certificate2("MyCertPath.pfx", "MyCertPassword", X509KeyStorageFlags.Exportable);

 string samlRequest = request.GetSignedAuthRequest(signinCert, rsaXmlKey);
```
<br/>

#### Esempio Controller Invio:

```csharp
public IActionResult SPID()
{
    Developers.Italia.SPID.SAML.AuthRequestOptions options = new Developers.Italia.SPID.SAML.AuthRequestOptions()
    {
        //Request Unique Identifier
        UUID = Guid.NewGuid().ToString(),
        //Service Provider Unique Identifier (url like https://www.dotnetcode.it can be ok)
        SPUID = "https://www.dotnetcode.it",
        //SPID Authentication Level (1/2/3)
        SPIDLevel = Developers.Italia.SPID.SAML.SPIDLevel.SPIDL1,
        //Identity Provider Url For SAML Request
        Destination = "https://spidposte.test.poste.it/jod-fs/ssoservicepost",
        //Service Provider Consumer Service Index - Normally Callback URL (refer to Service Provider Metadata)
        AssertionConsumerServiceIndex = 0,
        //Service Provider Consuming Service Index - Normally Requested User Data(refer to Service Provider Metadata)
        AttributeConsumingServiceIndex = 0
    };

    Developers.Italia.SPID.SAML.AuthRequest request = new Developers.Italia.SPID.SAML.AuthRequest(options);


    string rsaXmlKey = "<![CDATA[<RSAKeyValue><Modulus>rBPwxOB3QM+Rhz+/...</RSAKeyValue>";
    X509Certificate2 signinCert = new X509Certificate2("MyCertPath.pfx", "MyCertPassword", X509KeyStorageFlags.Exportable);

    string samlRequest = request.GetSignedAuthRequest(signinCert, rsaXmlKey);


            
    string returnUrl = "/";

    if (!string.IsNullOrEmpty(HttpContext.Request.Query["redirectUrl"]))
    {
        returnUrl = HttpContext.Request.Query["redirectUrl"];
    }

    ViewData["FormUrlAction"] = options.Destination;
    ViewData["SAMLRequest"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(samlRequest));
    ViewData["RelayState"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(returnUrl));

    return View();
}
```
<br/>

#### Esempio Controller Ricezione:

```csharp
// POST: SpidSaml/ACS
[HttpPost]
public ActionResult ACS(IFormCollection collection)
{
    string samlResponse = "";
    string redirect = "";
    SAML.AuthResponse resp = new SAML.AuthResponse();
    try
    {
        samlResponse = Encoding.UTF8.GetString(Convert.FromBase64String(collection["SAMLResponse"]));
        redirect = Encoding.UTF8.GetString(Convert.FromBase64String(collection["RelayState"]));
                
        resp.Deserialize(samlResponse);
                
    }
    catch (Exception ex)
    {
        //TODO LOG
    }

    ViewData["SAMLResponse"] = resp;
    ViewData["RelayState"] = redirect;
    return View();
}
}
```

<br/><br/>
## Demo:
E' possibile utilizzare una demo funzionante, basta scaricare da GitHub la cartella Developers.Italia.SPID.Test.AspNetCore2 che si trova sotto la directory Test, aprire la finestra di comando ed eseguire i seguenti comandi:

```shell
> dotnet build
> dotnet run
```

Una volta finito basterà andare all'indirizzo https://localhost:44355 sul browser e provare l'autenticazione tramite Poste.IT.

**N.B.**:Il certificato è self signed quindi bisogna bypassare i controlli del browser se presenti.

<br/><br/><br/>


## Documenti Utili:
Di seguito sono presenti alcuni link tecnici Utili:

## Regole Tecniche:
http://spid-regole-tecniche.readthedocs.io/en/latest/index.html

## Raccolta Documenti:
http://spid-regole-tecniche.readthedocs.io/en/latest/index.html
