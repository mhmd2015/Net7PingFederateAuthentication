using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.AspNetCore.Authentication;

namespace MasterBlazor.AspNetCore.Authentication.PingFederate
{
	public class RedirectContext : PropertiesContext<PingFederateOptions>
	{
		public WsFederationMessage ProtocolMessage
		{
			get;
			set;
		}

		public bool Handled
		{
			get;
			private set;
		}

		public RedirectContext(HttpContext context, AuthenticationScheme scheme, PingFederateOptions options, AuthenticationProperties properties)
			: base(context, scheme, options, properties)
		{
		}

		public void HandleResponse()
		{
			Handled = true;
		}
	}
}
