using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPCWare.SPTest.SAML.Security.Saml20.Metadata
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
    public partial class EntityDescriptorOrganization
    {

        private EntityDescriptorOrganizationOrganizationName organizationNameField;

        private EntityDescriptorOrganizationOrganizationURL organizationURLField;

        private EntityDescriptorOrganizationOrganizationDisplayName organizationDisplayNameField;

        public EntityDescriptorOrganizationOrganizationName OrganizationName
        {
            get
            {
                return this.organizationNameField;
            }
            set
            {
                this.organizationNameField = value;
            }
        }     

        public EntityDescriptorOrganizationOrganizationDisplayName OrganizationDisplayName
        {
            get
            {
                return this.organizationDisplayNameField;
            }
            set
            {
                this.organizationDisplayNameField = value;
            }
        }

        public EntityDescriptorOrganizationOrganizationURL OrganizationURL
        {
            get
            {
                return this.organizationURLField;
            }
            set
            {
                this.organizationURLField = value;
            }
        }
    }
}
