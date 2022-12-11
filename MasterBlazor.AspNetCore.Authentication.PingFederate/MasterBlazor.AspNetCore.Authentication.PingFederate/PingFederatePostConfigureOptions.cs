using System;
using System.Net.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.AspNetCore.Authentication;

namespace MasterBlazor.AspNetCore.Authentication.PingFederate
{
	public class PingFederatePostConfigureOptions : IPostConfigureOptions<PingFederateOptions>
	{
		private readonly IDataProtectionProvider _dp;

		public PingFederatePostConfigureOptions(IDataProtectionProvider dataProtection)
		{
			_dp = dataProtection;
		}

		public void PostConfigure(string name, PingFederateOptions options)
		{
			options.DataProtectionProvider = options.DataProtectionProvider ?? _dp;
			if (string.IsNullOrEmpty(options.SignOutScheme))
			{
				options.SignOutScheme = options.SignInScheme;
			}
			if (options.StateDataFormat == null)
			{
				IDataProtector protector = options.DataProtectionProvider.CreateProtector(typeof(PingFederateHandler).FullName, name, "v1");
				options.StateDataFormat = new PropertiesDataFormat(protector);
			}
			if (!options.CallbackPath.HasValue && !string.IsNullOrEmpty(options.Wreply) && Uri.TryCreate(options.Wreply, UriKind.Absolute, out var result))
			{
				options.CallbackPath = PathString.FromUriComponent(result);
			}
			if (string.IsNullOrEmpty(options.TokenValidationParameters.ValidAudience))
			{
				options.TokenValidationParameters.ValidAudience = options.Wtrealm;
			}
			if (options.Backchannel == null)
			{
				options.Backchannel = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler());
				options.Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core PingFederate handler");
				options.Backchannel.Timeout = options.BackchannelTimeout;
				options.Backchannel.MaxResponseContentBufferSize = 10485760L;
			}
			if (options.ConfigurationManager != null)
			{
				return;
			}
			if (options.Configuration != null)
			{
				options.ConfigurationManager = new StaticConfigurationManager<WsFederationConfiguration>(options.Configuration);
			}
			else if (!string.IsNullOrEmpty(options.MetadataAddress))
			{
				if (options.RequireHttpsMetadata && !options.MetadataAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidOperationException("The MetadataAddress must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.");
				}
				options.ConfigurationManager = new ConfigurationManager<WsFederationConfiguration>(options.MetadataAddress, new WsFederationConfigurationRetriever(), new HttpDocumentRetriever(options.Backchannel)
				{
					RequireHttps = options.RequireHttpsMetadata
				});
			}
		}
	}
}
