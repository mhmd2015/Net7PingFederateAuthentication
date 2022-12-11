using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;

namespace MasterBlazor.AspNetCore.Authentication.PingFederate
{
	public class SecurityTokenValidatedContext : RemoteAuthenticationContext<PingFederateOptions>
	{
		public WsFederationMessage ProtocolMessage
		{
			get;
			set;
		}

		public SecurityToken SecurityToken
		{
			get;
			set;
		}

		public SecurityTokenValidatedContext(HttpContext context, AuthenticationScheme scheme, PingFederateOptions options, ClaimsPrincipal principal, AuthenticationProperties properties)
			: base(context, scheme, options, properties)
		{
			base.Principal = principal;
		}
	}
}
