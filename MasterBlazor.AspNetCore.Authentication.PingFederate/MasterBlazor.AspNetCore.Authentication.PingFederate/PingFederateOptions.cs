using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml;
using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.AspNetCore.Authentication;

namespace MasterBlazor.AspNetCore.Authentication.PingFederate
{
	public class PingFederateOptions : RemoteAuthenticationOptions
	{
		private ICollection<ISecurityTokenValidator> _securityTokenHandlers = new Collection<ISecurityTokenValidator>
		{
			new Saml2SecurityTokenHandler(),
			new SamlSecurityTokenHandler(),
			new JwtSecurityTokenHandler()
		};

		private TokenValidationParameters _tokenValidationParameters = new TokenValidationParameters();

		public WsFederationConfiguration Configuration
		{
			get;
			set;
		}

		public string MetadataAddress
		{
			get;
			set;
		}

		public IConfigurationManager<WsFederationConfiguration> ConfigurationManager
		{
			get;
			set;
		}

		public bool RefreshOnIssuerKeyNotFound
		{
			get;
			set;
		} = true;


		public bool SkipUnrecognizedRequests
		{
			get;
			set;
		}

		public new PingFederateEvents Events
		{
			get
			{
				return (PingFederateEvents)base.Events;
			}
			set
			{
				base.Events = value;
			}
		}

		public ICollection<ISecurityTokenValidator> SecurityTokenHandlers
		{
			get
			{
				return _securityTokenHandlers;
			}
			set
			{
				_securityTokenHandlers = value ?? throw new ArgumentNullException("SecurityTokenHandlers");
			}
		}

		public ISecureDataFormat<AuthenticationProperties> StateDataFormat
		{
			get;
			set;
		}

		public TokenValidationParameters TokenValidationParameters
		{
			get
			{
				return _tokenValidationParameters;
			}
			set
			{
				_tokenValidationParameters = value ?? throw new ArgumentNullException("TokenValidationParameters");
			}
		}

		public string Wreply
		{
			get;
			set;
		}

		public string SignOutWreply
		{
			get;
			set;
		}

		public string Wtrealm
		{
			get;
			set;
		}

		public bool UseTokenLifetime
		{
			get;
			set;
		} = true;


		public bool RequireHttpsMetadata
		{
			get;
			set;
		} = true;


		public bool AllowUnsolicitedLogins
		{
			get;
			set;
		}

		public PathString RemoteSignOutPath
		{
			get;
			set;
		}

		public string SignOutScheme
		{
			get;
			set;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool SaveTokens
		{
			get;
			set;
		}

		public PingFederateOptions()
		{
			base.CallbackPath = "/signin-wsfed";
			RemoteSignOutPath = "/signin-wsfed";
			Events = new PingFederateEvents();
		}

		public override void Validate()
		{
			base.Validate();
			if (ConfigurationManager == null)
			{
				throw new InvalidOperationException("Provide MetadataAddress, Configuration, or ConfigurationManager to WsFederationOptions");
			}
		}
	}
}
