using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authentication;

namespace MasterBlazor.AspNetCore.Authentication.PingFederate
{
	internal static class Resources
	{
		private static ResourceManager s_resourceManager;

		internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(Resources)));

		internal static CultureInfo Culture
		{
			get;
			set;
		}

		internal static string Exception_MissingDescriptor => GetResourceString("Exception_MissingDescriptor");

		internal static string Exception_NoTokenValidatorFound => GetResourceString("Exception_NoTokenValidatorFound");

		internal static string Exception_OptionMustBeProvided => GetResourceString("Exception_OptionMustBeProvided");

		internal static string Exception_ValidatorHandlerMismatch => GetResourceString("Exception_ValidatorHandlerMismatch");

		internal static string SignInMessageTokenIsMissing => GetResourceString("SignInMessageTokenIsMissing");

		internal static string SignInMessageWresultIsMissing => GetResourceString("SignInMessageWresultIsMissing");

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static string GetResourceString(string resourceKey, string defaultValue = null)
		{
			return ResourceManager.GetString(resourceKey, Culture);
		}

		private static string GetResourceString(string resourceKey, string[] formatterNames)
		{
			string text = GetResourceString(resourceKey);
			if (formatterNames != null)
			{
				for (int i = 0; i < formatterNames.Length; i++)
				{
					text = text.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
				}
			}
			return text;
		}

		internal static string FormatException_OptionMustBeProvided(object p0)
		{
			return string.Format(Culture, GetResourceString("Exception_OptionMustBeProvided"), p0);
		}
	}
}
