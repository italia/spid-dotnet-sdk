Developers.Italia.SPID.SAML
=

Questa libreria permette di interfacciare un'applicazione ASP.NET con un IdP (Identity Provider) abilitato a **SPID** ([Sistema Pubblico di Identità Digitale](https://www.spid.gov.it])).

Tramite questa libreria è possibile effettuare l'autenticazione di un Utente registrato presso un Identity Provider attivo e richiedere alcuni dati dell'Utente (Cittadino)

### Step 1
#### Creazione Certificati per firma digitale
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

# Regole Tecniche:
http://spid-regole-tecniche.readthedocs.io/en/latest/index.html
