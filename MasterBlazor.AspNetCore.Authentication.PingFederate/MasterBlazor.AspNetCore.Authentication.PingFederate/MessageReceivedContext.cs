using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.AspNetCore.Authentication;

namespace MasterBlazor.AspNetCore.Authentication.PingFederate
{
	public class MessageReceivedContext : RemoteAuthenticationContext<PingFederateOptions>
	{
		public WsFederationMessage ProtocolMessage
		{
			get;
			set;
		}

		public MessageReceivedContext(HttpContext context, AuthenticationScheme scheme, PingFederateOptions options, AuthenticationProperties properties)
			: base(context, scheme, options, properties)
		{
		}
	}
}
