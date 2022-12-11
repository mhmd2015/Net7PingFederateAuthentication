using System;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.AspNetCore.Authentication;

namespace MasterBlazor.AspNetCore.Authentication.PingFederate
{
	public class AuthenticationFailedContext : RemoteAuthenticationContext<PingFederateOptions>
	{
		public WsFederationMessage ProtocolMessage
		{
			get;
			set;
		}

		public Exception Exception
		{
			get;
			set;
		}

		public AuthenticationFailedContext(HttpContext context, AuthenticationScheme scheme, PingFederateOptions options)
			: base(context, scheme, options, new AuthenticationProperties())
		{
		}
	}
}
