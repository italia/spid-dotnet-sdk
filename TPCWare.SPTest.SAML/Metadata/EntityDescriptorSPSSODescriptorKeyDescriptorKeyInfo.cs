using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPCWare.SPTest.SAML.Security.Saml20.Metadata
{
    public partial class EntityDescriptorSPSSODescriptorKeyDescriptorKeyInfo
    {
        private EntityDescriptorSPSSODescriptorKeyDescriptorKeyInfoX509Data X509DataField;
        private string valueField;

        public EntityDescriptorSPSSODescriptorKeyDescriptorKeyInfoX509Data X509Data
        {
            get
            {
                return this.X509DataField;
            }
            set
            {
                this.X509DataField = value;
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
