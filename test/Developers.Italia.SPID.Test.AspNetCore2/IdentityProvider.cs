using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Developers.Italia.SPID.Test.AspNetCore2
{

    [XmlRoot(ElementName = "IdentityProvider")]
    public class IdentityProvider
    {
        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "IdentityProviderLoginPostUrl")]
        public string IdentityProviderLoginPostUrl { get; set; }

        [XmlElement(ElementName = "IdentityProviderLogoutPostUrl")]
        public string IdentityProviderLogoutPostUrl { get; set; }

        [XmlElement(ElementName = "IdentityProviderCertPath")]
        public string IdentityProviderCertPath { get; set; }

        [XmlElement(ElementName = "ServiceProviderId")]
        public string ServiceProviderId { get; set; }

        [XmlElement(ElementName = "ServiceProviderCertPath")]
        public string ServiceProviderCertPath { get; set; }

        [XmlElement(ElementName = "ServiceProviderPrivatekey")]
        public string ServiceProviderPrivatekey { get; set; }

    }


}
