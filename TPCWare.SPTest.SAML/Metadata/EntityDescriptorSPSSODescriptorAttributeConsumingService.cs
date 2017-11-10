using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPCWare.SPTest.SAML.Security.Saml20.Metadata
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorSPSSODescriptorAttributeConsumingService
    {

        private EntityDescriptorSPSSODescriptorAttributeConsumingServiceServiceName serviceNameField;

        //private EntityDescriptorSPSSODescriptorAttributeConsumingServiceRequestedAttribute[] requestedAttributeField;
        private List<EntityDescriptorSPSSODescriptorAttributeConsumingServiceRequestedAttribute> requestedAttributeField;

        private byte indexField;

        /// <remarks/>
        public EntityDescriptorSPSSODescriptorAttributeConsumingServiceServiceName ServiceName
        {
            get
            {
                return this.serviceNameField;
            }
            set
            {
                this.serviceNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("RequestedAttribute")]
        //public EntityDescriptorSPSSODescriptorAttributeConsumingServiceRequestedAttribute[] RequestedAttribute
        public List<EntityDescriptorSPSSODescriptorAttributeConsumingServiceRequestedAttribute> RequestedAttribute
        {
            get
            {
                return this.requestedAttributeField;
            }
            set
            {
                this.requestedAttributeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte index
        {
            get
            {
                return this.indexField;
            }
            set
            {
                this.indexField = value;
            }
        }
    }
}
