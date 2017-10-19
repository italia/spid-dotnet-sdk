using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPCWare.SPTest.SAML.Security.Saml20.Metadata
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata", IsNullable = false)]
    public partial class EntityDescriptor
    {

        private string signatureField;

        private EntityDescriptorSPSSODescriptor sPSSODescriptorField;

        private string entityIDField;

        private EntityDescriptorOrganization sOrganizationField;

        private string IDField;

        public string Signature
        {
            get
            {
                return this.signatureField;
            }
            set
            {
                this.signatureField = value;
            }
        }

        /// <remarks/>
        public EntityDescriptorSPSSODescriptor SPSSODescriptor
        {
            get
            {
                return this.sPSSODescriptorField;
            }
            set
            {
                this.sPSSODescriptorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string entityID
        {
            get
            {
                return this.entityIDField;
            }
            set
            {
                this.entityIDField = value;
            }
        }

        public EntityDescriptorOrganization Organization
        {
            get
            {
                return this.sOrganizationField;
            }
            set
            {
                this.sOrganizationField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            get
            {
                return this.IDField;
            }
            set
            {
                this.IDField = value;
            }
        }
    }
}
