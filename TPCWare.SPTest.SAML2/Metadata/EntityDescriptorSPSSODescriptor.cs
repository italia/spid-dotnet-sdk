using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPCWare.SPTest.SAML.Security.Saml20.Metadata
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorSPSSODescriptor
    {

        private EntityDescriptorSPSSODescriptorKeyDescriptor keyDescriptorField;
        private EntityDescriptorSPSSODescriptorSingleLogoutService SingleLogoutServiceField;
        private EntityDescriptorSPSSODescriptorNameIDFormat NameIdFormatField;

        //private EntityDescriptorSPSSODescriptorAssertionConsumerService[] assertionConsumerServiceField;
        private List<EntityDescriptorSPSSODescriptorAssertionConsumerService> assertionConsumerServiceField;

        //private EntityDescriptorSPSSODescriptorAttributeConsumingService[] attributeConsumingServiceField;
        private List<EntityDescriptorSPSSODescriptorAttributeConsumingService> attributeConsumingServiceField;

        private string protocolSupportEnumerationField;

        private bool WantAssertionsSignedField;

        private bool authnRequestsSignedField;

        /// <remarks/>
        public EntityDescriptorSPSSODescriptorKeyDescriptor KeyDescriptor
        {
            get
            {
                return this.keyDescriptorField;
            }
            set
            {
                this.keyDescriptorField = value;
            }
        }
        public EntityDescriptorSPSSODescriptorSingleLogoutService SingleLogoutService
        {
            get
            {
                return this.SingleLogoutServiceField;
            }
            set
            {
                this.SingleLogoutServiceField = value;
            }
        }
        public EntityDescriptorSPSSODescriptorNameIDFormat NameIDFormat
        {
            get
            {
                return this.NameIdFormatField;
            }
            set
            {
                this.NameIdFormatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AssertionConsumerService")]
        public List<EntityDescriptorSPSSODescriptorAssertionConsumerService> AssertionConsumerService
        {
            get
            {
                return this.assertionConsumerServiceField;
            }
            set
            {
                this.assertionConsumerServiceField = value;
            }
        }


        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AttributeConsumingService")]
        //public EntityDescriptorSPSSODescriptorAttributeConsumingService[] AttributeConsumingService
        public List<EntityDescriptorSPSSODescriptorAttributeConsumingService> AttributeConsumingService
        {
            get
            {
                return this.attributeConsumingServiceField;
            }
            set
            {
                this.attributeConsumingServiceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string protocolSupportEnumeration
        {
            get
            {
                return this.protocolSupportEnumerationField;
            }
            set
            {
                this.protocolSupportEnumerationField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool WantAssertionsSigned
        {
            get
            {
                return this.WantAssertionsSignedField;
            }
            set
            {
                this.WantAssertionsSignedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool AuthnRequestsSigned
        {
            get
            {
                return this.authnRequestsSignedField;
            }
            set
            {
                this.authnRequestsSignedField = value;
            }
        }
    }
}
