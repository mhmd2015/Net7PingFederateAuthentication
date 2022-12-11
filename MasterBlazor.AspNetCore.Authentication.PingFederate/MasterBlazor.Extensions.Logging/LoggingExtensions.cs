using System;

namespace Microsoft.Extensions.Logging
{
	internal static class LoggingExtensions
	{
		private static Action<ILogger, Exception> _signInWithoutWResult;

		private static Action<ILogger, Exception> _signInWithoutToken;

		private static Action<ILogger, Exception> _exceptionProcessingMessage;

		private static Action<ILogger, string, Exception> _malformedRedirectUri;

		private static Action<ILogger, Exception> _remoteSignOutHandledResponse;

		private static Action<ILogger, Exception> _remoteSignOutSkipped;

		private static Action<ILogger, Exception> _remoteSignOut;

		static LoggingExtensions()
		{
			_signInWithoutWResult = LoggerMessage.Define(LogLevel.Debug, new EventId(1, "SignInWithoutWResult"), "Received a sign-in message without a WResult.");
			_signInWithoutToken = LoggerMessage.Define(LogLevel.Debug, new EventId(2, "SignInWithoutToken"), "Received a sign-in message without a token.");
			_exceptionProcessingMessage = LoggerMessage.Define(LogLevel.Error, new EventId(3, "ExceptionProcessingMessage"), "Exception occurred while processing message.");
			_malformedRedirectUri = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(4, "MalformedRedirectUri"), "The sign-out redirect URI '{0}' is malformed.");
			_remoteSignOutHandledResponse = LoggerMessage.Define(LogLevel.Debug, new EventId(5, "RemoteSignOutHandledResponse"), "RemoteSignOutContext.HandledResponse");
			_remoteSignOutSkipped = LoggerMessage.Define(LogLevel.Debug, new EventId(6, "RemoteSignOutSkipped"), "RemoteSignOutContext.Skipped");
			_remoteSignOut = LoggerMessage.Define(LogLevel.Information, new EventId(7, "RemoteSignOut"), "Remote signout request processed.");
		}

		public static void SignInWithoutWResult(this ILogger logger)
		{
			_signInWithoutWResult(logger, null);
		}

		public static void SignInWithoutToken(this ILogger logger)
		{
			_signInWithoutToken(logger, null);
		}

		public static void ExceptionProcessingMessage(this ILogger logger, Exception ex)
		{
			_exceptionProcessingMessage(logger, ex);
		}

		public static void MalformedRedirectUri(this ILogger logger, string uri)
		{
			_malformedRedirectUri(logger, uri, null);
		}

		public static void RemoteSignOutHandledResponse(this ILogger logger)
		{
			_remoteSignOutHandledResponse(logger, null);
		}

		public static void RemoteSignOutSkipped(this ILogger logger)
		{
			_remoteSignOutSkipped(logger, null);
		}

		public static void RemoteSignOut(this ILogger logger)
		{
			_remoteSignOut(logger, null);
		}
	}
}
