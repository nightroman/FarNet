
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using FarNet;

namespace PowerShellFar
{
	static class NativeMethods
	{
		// From System.Management.Automation.HostUtilities
		[DllImport("credui", EntryPoint = "CredUIPromptForCredentialsW", CharSet = CharSet.Unicode)]
		private static extern CredUIReturnCodes CredUIPromptForCredentials(ref CREDUI_INFO pUiInfo, string pszTargetName, IntPtr Reserved, int dwAuthError, StringBuilder pszUserName, int ulUserNameMaxChars, StringBuilder pszPassword, int ulPasswordMaxChars, ref int pfSave, CREDUI_FLAGS dwFlags);

		// From System.Management.Automation.HostUtilities
		[Flags]
		private enum CREDUI_FLAGS
		{
			ALWAYS_SHOW_UI = 0x80,
			COMPLETE_USERNAME = 0x800,
			DO_NOT_PERSIST = 2,
			EXCLUDE_CERTIFICATES = 8,
			EXPECT_CONFIRMATION = 0x20000,
			GENERIC_CREDENTIALS = 0x40000,
			INCORRECT_PASSWORD = 1,
			KEEP_USERNAME = 0x100000,
			PASSWORD_ONLY_OK = 0x200,
			PERSIST = 0x1000,
			REQUEST_ADMINISTRATOR = 4,
			REQUIRE_CERTIFICATE = 0x10,
			REQUIRE_SMARTCARD = 0x100,
			SERVER_CREDENTIAL = 0x4000,
			SHOW_SAVE_CHECK_BOX = 0x40,
			USERNAME_TARGET_CREDENTIALS = 0x80000,
			VALIDATE_USERNAME = 0x400
		}

		// From System.Management.Automation.HostUtilities
		[StructLayout(LayoutKind.Sequential)]
		private struct CREDUI_INFO
		{
			public int cbSize;
			public IntPtr hwndParent;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszMessageText;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszCaptionText;
			public IntPtr hbmBanner;
		}

		// From System.Management.Automation.HostUtilities
		private enum CredUIReturnCodes
		{
			ERROR_CANCELLED = 0x4c7,
			ERROR_INSUFFICIENT_BUFFER = 0x7a,
			ERROR_INVALID_ACCOUNT_NAME = 0x523,
			ERROR_INVALID_FLAGS = 0x3ec,
			ERROR_INVALID_PARAMETER = 0x57,
			ERROR_NO_SUCH_LOGON_SESSION = 0x520,
			ERROR_NOT_FOUND = 0x490,
			NO_ERROR = 0
		}

		// From System.Management.Automation.HostUtilities, adapted
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public static PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
		{
			if (string.IsNullOrEmpty(caption))
			{
				caption = Res.Me + " Credential Request";
			}
			if (string.IsNullOrEmpty(message))
			{
				message = "Enter your credentials.";
			}
			CREDUI_INFO cREDUI_INFO = default(CREDUI_INFO);
			cREDUI_INFO.pszCaptionText = caption;
			cREDUI_INFO.pszMessageText = message;
			StringBuilder stringBuilder = new StringBuilder(userName, 513);
			StringBuilder stringBuilder2 = new StringBuilder(256);
			bool value = false;
			int num = Convert.ToInt32(value);
			cREDUI_INFO.cbSize = Marshal.SizeOf(cREDUI_INFO);
			cREDUI_INFO.hwndParent = Far.Api.UI.MainWindowHandle; //! works for conemu, too, but the effect is as if we use IntPtr.Zero
			CREDUI_FLAGS cREDUI_FLAGS = CREDUI_FLAGS.DO_NOT_PERSIST;
			if ((allowedCredentialTypes & PSCredentialTypes.Domain) != PSCredentialTypes.Domain)
			{
				cREDUI_FLAGS |= CREDUI_FLAGS.GENERIC_CREDENTIALS;
				if ((options & PSCredentialUIOptions.AlwaysPrompt) == PSCredentialUIOptions.AlwaysPrompt)
				{
					cREDUI_FLAGS |= CREDUI_FLAGS.ALWAYS_SHOW_UI;
				}
			}
			CredUIReturnCodes credUIReturnCodes = CredUIReturnCodes.ERROR_INVALID_PARAMETER;
			if (stringBuilder.Length <= 513 && stringBuilder2.Length <= 256)
			{
				credUIReturnCodes = CredUIPromptForCredentials(ref cREDUI_INFO, targetName, IntPtr.Zero, 0, stringBuilder, 513, stringBuilder2, 256, ref num, cREDUI_FLAGS);
			}
			PSCredential pSCredential;
			if (credUIReturnCodes == CredUIReturnCodes.NO_ERROR)
			{
				string text = null;
				if (stringBuilder != null)
				{
					text = stringBuilder.ToString();
				}
				SecureString secureString = new SecureString();
				for (int i = 0; i < stringBuilder2.Length; i++)
				{
					secureString.AppendChar(stringBuilder2[i]);
					stringBuilder2[i] = '\0';
				}
				if (!string.IsNullOrEmpty(text))
				{
					pSCredential = new PSCredential(text, secureString);
				}
				else
				{
					pSCredential = null;
				}
			}
			else
			{
				//if (credUIReturnCodes != CredUIReturnCodes.ERROR_CANCELLED)
				//    throw new InvalidOperationException("Getting credentials error: " + credUIReturnCodes);

				pSCredential = null;
			}
			return pSCredential;
		}
	}
}
