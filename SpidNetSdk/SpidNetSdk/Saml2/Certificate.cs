using System.Security.Cryptography.X509Certificates;

namespace SpidNetSdk.Saml2
{
    public class Certificate
    {
        public X509Certificate2 cert;

        public void LoadCertificate(string certificate)
        {
            cert = new X509Certificate2();
            cert.Import(StringToByteArray(certificate));
        }

        public void LoadCertificate(byte[] certificate)
        {
            cert = new X509Certificate2();
            cert.Import(certificate);
        }

        private byte[] StringToByteArray(string st)
        {
            byte[] bytes = new byte[st.Length];
            for (int i = 0; i < st.Length; i++)
            {
                bytes[i] = (byte)st[i];
            }
            return bytes;
        }
    }
}
