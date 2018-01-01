using System;
using System.Collections.Generic;
using System.Text;

namespace Italia.Spid.Authentication.IdP
{
    public class SamlIdentityProviderSettings
    {
        /// <summary>
        /// The single sign on service URL
        /// </summary>
        public const string SingleSignOnServiceUrl = "SingleSignOnServiceUrl";

        /// <summary>
        /// The single logout service URL
        /// </summary>
        public const string SingleLogoutServiceUrl = "SingleLogoutServiceUrl";

        /// <summary>
        /// The subject name identifier remove text
        /// </summary>
        public const string SubjectNameIdRemoveText = "SubjectNameIdRemoveText";

        /// <summary>
        /// The date time format
        /// </summary>
        public const string DateTimeFormat = "DateTimeFormat";

        /// <summary>
        /// The now delta
        /// </summary>
        public const string NowDelta = "NowDelta";

        /// <summary>
        /// The assertion consumer service index
        /// </summary>
        public const string AssertionConsumerServiceIndex = "AssertionConsumerServiceIndex";

        /// <summary>
        /// The attribute consuming service index
        /// </summary>
        public const string AttributeConsumingServiceIndex = "AttributeConsumingServiceIndex";
        

        /// <summary>
        /// Gets the default settings.
        /// </summary>
        /// <value>
        /// The default settings.
        /// </value>
        public static Dictionary<string, string> DefaultSettings
        {
            get
            {
                return new Dictionary<string, string>(){
                    { SingleSignOnServiceUrl,string.Empty },
                    { SingleLogoutServiceUrl,string.Empty },
                    { SubjectNameIdRemoveText,string.Empty },
                    { DateTimeFormat,"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'" },
                    { NowDelta,"0" },
                    { AssertionConsumerServiceIndex,"0" },
                    { AttributeConsumingServiceIndex,"0" }
                };
            }
        }

    }
}
