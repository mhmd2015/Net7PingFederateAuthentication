using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;

namespace MasterBlazor.AspNetCore.Authentication.PingFederate
{
	public class PingFederateHandler : RemoteAuthenticationHandler<PingFederateOptions>, IAuthenticationSignOutHandler, IAuthenticationHandler
	{
		private const string CorrelationProperty = ".xsrf";

		private WsFederationConfiguration _configuration;

		protected new PingFederateEvents Events
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

		public PingFederateHandler(IOptionsMonitor<PingFederateOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
			: base(options, logger, encoder, clock)
		{
		}

		protected override Task<object> CreateEventsAsync()
		{
			return Task.FromResult((object)new PingFederateEvents());
		}

		public override Task<bool> HandleRequestAsync()
		{
			if (base.Options.RemoteSignOutPath.HasValue && base.Options.RemoteSignOutPath == base.Request.Path && HttpMethods.IsGet(base.Request.Method) && string.Equals((string)base.Request.Query["wa"], "wsignoutcleanup1.0", StringComparison.OrdinalIgnoreCase))
			{
				return HandleRemoteSignOutAsync();
			}
			return base.HandleRequestAsync();
		}

		protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
		{
			if (_configuration == null)
			{
				_configuration = await base.Options.ConfigurationManager.GetConfigurationAsync(base.Context.RequestAborted);
			}
			if (string.IsNullOrEmpty(properties.RedirectUri))
			{
				properties.RedirectUri = base.OriginalPathBase + base.OriginalPath + base.Request.QueryString;
			}
			WsFederationMessage pingFederateMessage = new WsFederationMessage
			{
				IssuerAddress = (_configuration.TokenEndpoint ?? string.Empty),
				Wtrealm = base.Options.Wtrealm,
				Wa = "wsignin1.0"
			};
			if (!string.IsNullOrEmpty(base.Options.Wreply))
			{
				pingFederateMessage.Wreply = base.Options.Wreply;
			}
			else
			{
				pingFederateMessage.Wreply = BuildRedirectUri(base.Options.CallbackPath);
			}
			GenerateCorrelationId(properties);
			RedirectContext redirectContext = new RedirectContext(base.Context, base.Scheme, base.Options, properties)
			{
				ProtocolMessage = pingFederateMessage
			};
			await Events.RedirectToIdentityProvider(redirectContext);
			if (!redirectContext.Handled)
			{
				pingFederateMessage = redirectContext.ProtocolMessage;
				if (!string.IsNullOrEmpty(pingFederateMessage.Wctx))
				{
					properties.Items[PingFederateDefaults.UserstatePropertiesKey] = pingFederateMessage.Wctx;
				}
				pingFederateMessage.Wctx = Uri.EscapeDataString(base.Options.StateDataFormat.Protect(properties));
				string text = pingFederateMessage.CreateSignInUrl();
				if (!Uri.IsWellFormedUriString(text, UriKind.Absolute))
				{
					base.Logger.MalformedRedirectUri(text);
				}
				base.Response.Redirect(text);
			}
		}

		protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
		{
			WsFederationMessage pingFederateMessage = null;
			AuthenticationProperties properties = null;
			if (HttpMethods.IsPost(base.Request.Method) && !string.IsNullOrEmpty(base.Request.ContentType) && base.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) && base.Request.Body.CanRead)
			{
				pingFederateMessage = new WsFederationMessage((await base.Request.ReadFormAsync()).Select<KeyValuePair<string, StringValues>, KeyValuePair<string, string[]>>((KeyValuePair<string, StringValues> pair) => new KeyValuePair<string, string[]>(pair.Key, pair.Value)));
			}
			if (pingFederateMessage == null || !pingFederateMessage.IsSignInMessage)
			{
				if (base.Options.SkipUnrecognizedRequests)
				{
					return HandleRequestResult.SkipHandler();
				}
				return HandleRequestResult.Fail("No message.");
			}
			try
			{
				string wctx = pingFederateMessage.Wctx;
				properties = base.Options.StateDataFormat.Unprotect(wctx);
				if (properties == null)
				{
					if (!base.Options.AllowUnsolicitedLogins)
					{
						return HandleRequestResult.Fail("Unsolicited logins are not allowed.");
					}
				}
				else
				{
					properties.Items.TryGetValue(PingFederateDefaults.UserstatePropertiesKey, out var value);
					pingFederateMessage.Wctx = value;
				}
				MessageReceivedContext messageReceivedContext = new MessageReceivedContext(base.Context, base.Scheme, base.Options, properties)
				{
					ProtocolMessage = pingFederateMessage
				};
				await Events.MessageReceived(messageReceivedContext);
				if (messageReceivedContext.Result != null)
				{
					return messageReceivedContext.Result;
				}
				pingFederateMessage = messageReceivedContext.ProtocolMessage;
				properties = messageReceivedContext.Properties;
				if (properties.Items.TryGetValue(".xsrf", out var _) && !ValidateCorrelationId(properties))
				{
					return HandleRequestResult.Fail("Correlation failed.", properties);
				}
				if (pingFederateMessage.Wresult == null)
				{
					base.Logger.SignInWithoutWResult();
					return HandleRequestResult.Fail(Resources.SignInMessageWresultIsMissing, properties);
				}
				string token = pingFederateMessage.GetToken();
				if (string.IsNullOrEmpty(token))
				{
					base.Logger.SignInWithoutToken();
					return HandleRequestResult.Fail(Resources.SignInMessageTokenIsMissing, properties);
				}
				SecurityTokenReceivedContext securityTokenReceivedContext = new SecurityTokenReceivedContext(base.Context, base.Scheme, base.Options, properties)
				{
					ProtocolMessage = pingFederateMessage
				};
				await Events.SecurityTokenReceived(securityTokenReceivedContext);
				if (securityTokenReceivedContext.Result != null)
				{
					return securityTokenReceivedContext.Result;
				}
				pingFederateMessage = securityTokenReceivedContext.ProtocolMessage;
				properties = messageReceivedContext.Properties;
				if (_configuration == null)
				{
					_configuration = await base.Options.ConfigurationManager.GetConfigurationAsync(base.Context.RequestAborted);
				}
				TokenValidationParameters tokenValidationParameters = base.Options.TokenValidationParameters.Clone();
				string[] array = new string[1]
				{
					_configuration.Issuer
				};
				IEnumerable<string> validIssuers;
				if (tokenValidationParameters.ValidIssuers != null)
				{
					validIssuers = tokenValidationParameters.ValidIssuers.Concat(array);
				}
				else
				{
					IEnumerable<string> enumerable = array;
					validIssuers = enumerable;
				}
				tokenValidationParameters.ValidIssuers = validIssuers;
				IEnumerable<SecurityKey> issuerSigningKeys;
				if (tokenValidationParameters.IssuerSigningKeys != null)
				{
					issuerSigningKeys = tokenValidationParameters.IssuerSigningKeys.Concat(_configuration.SigningKeys);
				}
				else
				{
					IEnumerable<SecurityKey> signingKeys = _configuration.SigningKeys;
					issuerSigningKeys = signingKeys;
				}
				tokenValidationParameters.IssuerSigningKeys = issuerSigningKeys;
				ClaimsPrincipal claimsPrincipal = null;
				SecurityToken validatedToken = null;
				foreach (ISecurityTokenValidator securityTokenHandler in base.Options.SecurityTokenHandlers)
				{
					if (securityTokenHandler.CanReadToken(token))
					{
						claimsPrincipal = securityTokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
						break;
					}
				}
				if (claimsPrincipal == null)
				{
					throw new SecurityTokenException(Resources.Exception_NoTokenValidatorFound);
				}
				if (base.Options.UseTokenLifetime && validatedToken != null)
				{
					DateTime validFrom = validatedToken.ValidFrom;
					if (validFrom != DateTime.MinValue)
					{
						properties.IssuedUtc = validFrom.ToUniversalTime();
					}
					DateTime validTo = validatedToken.ValidTo;
					if (validTo != DateTime.MinValue)
					{
						properties.ExpiresUtc = validTo.ToUniversalTime();
					}
					properties.AllowRefresh = false;
				}
				SecurityTokenValidatedContext securityTokenValidatedContext = new SecurityTokenValidatedContext(base.Context, base.Scheme, base.Options, claimsPrincipal, properties)
				{
					ProtocolMessage = pingFederateMessage,
					SecurityToken = validatedToken
				};
				await Events.SecurityTokenValidated(securityTokenValidatedContext);
				if (securityTokenValidatedContext.Result != null)
				{
					return securityTokenValidatedContext.Result;
				}
				claimsPrincipal = securityTokenValidatedContext.Principal;
				properties = securityTokenValidatedContext.Properties;
				return HandleRequestResult.Success(new AuthenticationTicket(claimsPrincipal, properties, base.Scheme.Name));
			}
			catch (Exception exception)
			{
				base.Logger.ExceptionProcessingMessage(exception);
				if (base.Options.RefreshOnIssuerKeyNotFound && exception is SecurityTokenSignatureKeyNotFoundException)
				{
					base.Options.ConfigurationManager.RequestRefresh();
				}
				AuthenticationFailedContext authenticationFailedContext = new AuthenticationFailedContext(base.Context, base.Scheme, base.Options)
				{
					ProtocolMessage = pingFederateMessage,
					Exception = exception
				};
				await Events.AuthenticationFailed(authenticationFailedContext);
				if (authenticationFailedContext.Result != null)
				{
					return authenticationFailedContext.Result;
				}
				return HandleRequestResult.Fail(exception, properties);
			}
		}

		public virtual async Task SignOutAsync(AuthenticationProperties properties)
		{
			string text = ResolveTarget(base.Options.ForwardSignOut);
			if (text != null)
			{
				await base.Context.SignOutAsync(text, properties);
				return;
			}
			if (_configuration == null)
			{
				_configuration = await base.Options.ConfigurationManager.GetConfigurationAsync(base.Context.RequestAborted);
			}
			WsFederationMessage pingFederateMessage = new WsFederationMessage
			{
				IssuerAddress = (_configuration.TokenEndpoint ?? string.Empty),
				Wtrealm = base.Options.Wtrealm,
				Wa = "wsignout1.0"
			};
			if (properties != null && !string.IsNullOrEmpty(properties.RedirectUri))
			{
				pingFederateMessage.Wreply = BuildRedirectUriIfRelative(properties.RedirectUri);
			}
			else if (!string.IsNullOrEmpty(base.Options.SignOutWreply))
			{
				pingFederateMessage.Wreply = BuildRedirectUriIfRelative(base.Options.SignOutWreply);
			}
			else if (!string.IsNullOrEmpty(base.Options.Wreply))
			{
				pingFederateMessage.Wreply = BuildRedirectUriIfRelative(base.Options.Wreply);
			}
			RedirectContext redirectContext = new RedirectContext(base.Context, base.Scheme, base.Options, properties)
			{
				ProtocolMessage = pingFederateMessage
			};
			await Events.RedirectToIdentityProvider(redirectContext);
			if (!redirectContext.Handled)
			{
				string text2 = redirectContext.ProtocolMessage.CreateSignOutUrl();
				if (!Uri.IsWellFormedUriString(text2, UriKind.Absolute))
				{
					base.Logger.MalformedRedirectUri(text2);
				}
				base.Response.Redirect(text2);
			}
		}

		protected virtual async Task<bool> HandleRemoteSignOutAsync()
		{
			WsFederationMessage message = new WsFederationMessage(base.Request.Query.Select<KeyValuePair<string, StringValues>, KeyValuePair<string, string[]>>((KeyValuePair<string, StringValues> pair) => new KeyValuePair<string, string[]>(pair.Key, pair.Value)));
			RemoteSignOutContext remoteSignOutContext = new RemoteSignOutContext(base.Context, base.Scheme, base.Options, message);
			await Events.RemoteSignOut(remoteSignOutContext);
			if (remoteSignOutContext.Result != null)
			{
				if (remoteSignOutContext.Result.Handled)
				{
					base.Logger.RemoteSignOutHandledResponse();
					return true;
				}
				if (remoteSignOutContext.Result.Skipped)
				{
					base.Logger.RemoteSignOutSkipped();
					return false;
				}
			}
			base.Logger.RemoteSignOut();
			await base.Context.SignOutAsync(base.Options.SignOutScheme);
			return true;
		}

		private string BuildRedirectUriIfRelative(string uri)
		{
			if (string.IsNullOrEmpty(uri))
			{
				return uri;
			}
			if (!uri.StartsWith("/", StringComparison.Ordinal))
			{
				return uri;
			}
			return BuildRedirectUri(uri);
		}
	}
}
