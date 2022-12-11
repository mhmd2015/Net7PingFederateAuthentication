using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace MasterBlazor.AspNetCore.Authentication.PingFederate
{
	public class PingFederateEvents : RemoteAuthenticationEvents
	{
		public Func<AuthenticationFailedContext, Task> OnAuthenticationFailed
		{
			get;
			set;
		} = (AuthenticationFailedContext context) => Task.CompletedTask;


		public Func<MessageReceivedContext, Task> OnMessageReceived
		{
			get;
			set;
		} = (MessageReceivedContext context) => Task.CompletedTask;


		public Func<RedirectContext, Task> OnRedirectToIdentityProvider
		{
			get;
			set;
		} = (RedirectContext context) => Task.CompletedTask;


		public Func<RemoteSignOutContext, Task> OnRemoteSignOut
		{
			get;
			set;
		} = (RemoteSignOutContext context) => Task.CompletedTask;


		public Func<SecurityTokenReceivedContext, Task> OnSecurityTokenReceived
		{
			get;
			set;
		} = (SecurityTokenReceivedContext context) => Task.CompletedTask;


		public Func<SecurityTokenValidatedContext, Task> OnSecurityTokenValidated
		{
			get;
			set;
		} = (SecurityTokenValidatedContext context) => Task.CompletedTask;


		public virtual Task AuthenticationFailed(AuthenticationFailedContext context)
		{
			return OnAuthenticationFailed(context);
		}

		public virtual Task MessageReceived(MessageReceivedContext context)
		{
			return OnMessageReceived(context);
		}

		public virtual Task RedirectToIdentityProvider(RedirectContext context)
		{
			return OnRedirectToIdentityProvider(context);
		}

		public virtual Task RemoteSignOut(RemoteSignOutContext context)
		{
			return OnRemoteSignOut(context);
		}

		public virtual Task SecurityTokenReceived(SecurityTokenReceivedContext context)
		{
			return OnSecurityTokenReceived(context);
		}

		public virtual Task SecurityTokenValidated(SecurityTokenValidatedContext context)
		{
			return OnSecurityTokenValidated(context);
		}
	}
}
