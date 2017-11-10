using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPCWare.SPTest.SAML.Security.Saml20.Metadata
{
    public partial class EntityDescriptorSPSSODescriptorKeyDescriptorKeyInfoX509Data
    {
        private EntityDescriptorSPSSODescriptorKeyDescriptorKeyInfoX509DataX509Certificate X509CertificateField;
        private string valueField;

        public EntityDescriptorSPSSODescriptorKeyDescriptorKeyInfoX509DataX509Certificate X509Certificate
        {
            get
            {
                return this.X509CertificateField;
            }
            set
            {
                this.X509CertificateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
}
