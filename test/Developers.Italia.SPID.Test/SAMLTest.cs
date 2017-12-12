using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;

namespace Developers.Italia.SPID.Test
{
    [TestClass]
    public class SAMLTest
    {
        [TestMethod]
        public void GetAuthRequest()
        {
            SAML.AuthRequestOptions requestOptions = new SAML.AuthRequestOptions()
            {
                AssertionConsumerServiceIndex = 0,
                AttributeConsumingServiceIndex = 0,
                Destination = "https://spidposte.test.poste.it/jod-fs/ssoservicepost",
                SPIDLevel = SAML.SPIDLevel.SPIDL1,
                SPUID = "dotnetcode.it",
                UUID = Guid.NewGuid().ToString()
            };

            SAML.AuthRequest request = new SAML.AuthRequest(requestOptions);
            string saml = request.GetAuthRequest();

        }
        [TestMethod]
        public void GetSignedAuthRequest()
        {
            SAML.AuthRequestOptions requestOptions = new SAML.AuthRequestOptions()
            {
                AssertionConsumerServiceIndex = 0,
                AttributeConsumingServiceIndex = 0,
                Destination = "https://spidposte.test.poste.it/jod-fs/ssoservicepost",
                SPIDLevel = SAML.SPIDLevel.SPIDL1,
                SPUID = "dotnetcode.it",
                UUID = Guid.NewGuid().ToString()
            };

            SAML.AuthRequest request = new SAML.AuthRequest(requestOptions);

            string certpath = string.Format("{0}\\{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["CertificatePath"].ToString());
            string privatekeypath = string.Format("{0}\\{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["PrivateKeyPath"].ToString());

            X509Certificate2 signinCert = new X509Certificate2("C:\\SourceCode\\spid-dotnet-sdk\\test\\Developers.Italia.SPID.Test\\Certificates\\Hackathon\\www_dotnetcode_it.pfx", "P@ssw0rd!", X509KeyStorageFlags.Exportable);

            //AsymmetricAlgorithm privateKey=new AsymmetricAlgorithm();



            string saml = request.GetSignedAuthRequest(signinCert);

        }

        [TestMethod]
        public void GetSignedLogoutRequest()
        {
            SAML.LogoutRequestOptions requestOptions = new SAML.LogoutRequestOptions()
            {
                
                Destination = "https://spidposte.test.poste.it/jod-fs/ssoservicepost",
                SessionId="SessionID",
                SPUID = "dotnetcode.it",
                UUID = Guid.NewGuid().ToString(),
                 LogoutLevel= SAML.LogoutLevel.User,
                  SubjectNameId= "SubjectNameId"
            };

            SAML.LogoutRequest request = new SAML.LogoutRequest(requestOptions);

            string certpath = string.Format("{0}\\{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["CertificatePath"].ToString());
            string privatekeypath = string.Format("{0}\\{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["PrivateKeyPath"].ToString());

            X509Certificate2 signinCert = new X509Certificate2("C:\\SourceCode\\spid-dotnet-sdk\\test\\Developers.Italia.SPID.Test\\Certificates\\Hackathon\\www_dotnetcode_it.pfx", "P@ssw0rd!", X509KeyStorageFlags.Exportable);

            //AsymmetricAlgorithm privateKey=new AsymmetricAlgorithm();



            string saml = request.GetSignedLogoutRequest(signinCert);

        }

    }
}
