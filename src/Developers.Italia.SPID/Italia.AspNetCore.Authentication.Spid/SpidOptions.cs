using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

namespace Italia.AspNetCore.Authentication.Spid
{
    public class SpidOptions : RemoteAuthenticationOptions
    {
        private const string DefaultStateCookieName = "__SpidState";

        private CookieBuilder _stateCookieBuilder;


        public SpidOptions()
        {
            CallbackPath = new PathString("/signin-spid");
            BackchannelTimeout = TimeSpan.FromSeconds(60);
            //Events = new TwitterEvents();

            //  ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);

            _stateCookieBuilder = new SpidCookieBuilder(this)
            {
                Name = DefaultStateCookieName,
                SecurePolicy = CookieSecurePolicy.SameAsRequest,
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
            };
        }



        /// <summary>
        /// Gets or sets the Service Provider Unique ID.
        /// </summary>
        /// <value>
        /// The spuid.
        /// </value>
        public string SPUID { get; set; }


        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        /// <value>
        /// The destination.
        /// </value>
        public string Destination { get; set; }

        /// <summary>
        /// Gets or sets the spid level.
        /// </summary>
        /// <value>
        /// The spid level.
        /// </value>
        public ushort SPIDLevel { get; set; }

        /// <summary>
        /// Gets or sets the index of the assertion consumer service.
        /// Refer to Service Provider Metadata Index Value On AssertionConsumerService
        /// </summary>
        /// <value>
        /// The index of the assertion consumer service.
        /// </value>
        public ushort AssertionConsumerServiceIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the attribute consuming service.
        /// </summary>
        /// <value>
        /// The index of the attribute consuming service.
        /// </value>
        public ushort AttributeConsumingServiceIndex { get; set; }

        public string SPIDCertPath { get; set; }
        public string SPIDCertPassword { get; set; }
        public string SPIDCertPrivateKey { get; set; }


        /// <summary>
        /// Determines the settings used to create the state cookie before the
        /// cookie gets added to the response.
        /// </summary>
        public CookieBuilder StateCookie
        {
            get => _stateCookieBuilder;
            set => _stateCookieBuilder = value ?? throw new ArgumentNullException(nameof(value));
        }

        private class SpidCookieBuilder : CookieBuilder
        {
            private readonly SpidOptions _spidOptions;

            public SpidCookieBuilder(SpidOptions spidOptions)
            {
                _spidOptions = spidOptions;
            }

            public override CookieOptions Build(HttpContext context, DateTimeOffset expiresFrom)
            {
                var options = base.Build(context, expiresFrom);
                if (!Expiration.HasValue)
                {
                    options.Expires = expiresFrom.Add(_spidOptions.RemoteAuthenticationTimeout);
                }
                return options;
            }
        }
    }
}
