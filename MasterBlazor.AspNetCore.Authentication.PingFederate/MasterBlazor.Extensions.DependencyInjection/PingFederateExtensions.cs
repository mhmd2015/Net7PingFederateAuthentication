using System;
using Microsoft.AspNetCore.Authentication;
using MasterBlazor.AspNetCore.Authentication.PingFederate;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class PingFederateExtensions
	{
		public static AuthenticationBuilder AddPingFederate(this AuthenticationBuilder builder)
		{
			return builder.AddPingFederate("PingFederate", delegate
			{
			});
		}

		public static AuthenticationBuilder AddPingFederate(this AuthenticationBuilder builder, Action<PingFederateOptions> configureOptions)
		{
			return builder.AddPingFederate("PingFederate", configureOptions);
		}

		public static AuthenticationBuilder AddPingFederate(this AuthenticationBuilder builder, string authenticationScheme, Action<PingFederateOptions> configureOptions)
		{
			return builder.AddPingFederate(authenticationScheme, "PingFederate", configureOptions);
		}

		public static AuthenticationBuilder AddPingFederate(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<PingFederateOptions> configureOptions)
		{
			builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<PingFederateOptions>, PingFederatePostConfigureOptions>());
			return builder.AddRemoteScheme<PingFederateOptions, PingFederateHandler>(authenticationScheme, displayName, configureOptions);
		}
	}
}
