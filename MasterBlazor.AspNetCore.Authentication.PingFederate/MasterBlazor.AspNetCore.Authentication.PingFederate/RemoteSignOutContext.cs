using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.AspNetCore.Authentication;

namespace MasterBlazor.AspNetCore.Authentication.PingFederate
{
	public class RemoteSignOutContext : RemoteAuthenticationContext<PingFederateOptions>
	{
		public WsFederationMessage ProtocolMessage
		{
			get;
			set;
		}

		public RemoteSignOutContext(HttpContext context, AuthenticationScheme scheme, PingFederateOptions options, WsFederationMessage message)
			: base(context, scheme, options, new AuthenticationProperties())
		{
			ProtocolMessage = message;
		}
	}
}
