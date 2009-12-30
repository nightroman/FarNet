/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

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
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public static PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
		{
			PSCredential credential = null;
			if (string.IsNullOrEmpty(caption))
			{
				caption = Res.Name + " Credential Request";
			}
			if (string.IsNullOrEmpty(message))
			{
				message = "Enter your credentials.";
			}
			CREDUI_INFO structure = new CREDUI_INFO();
			structure.pszCaptionText = caption;
			structure.pszMessageText = message;
			StringBuilder pszUserName = new StringBuilder(userName, 0x201);
			StringBuilder pszPassword = new StringBuilder(0x100);
			bool flag = false;
			int pfSave = Convert.ToInt32(flag);
			structure.cbSize = Marshal.SizeOf(structure);
			structure.hwndParent = A.Far.HWnd; //! works for conemu, too, but the effect is as if we use IntPtr.Zero
			CREDUI_FLAGS dwFlags = CREDUI_FLAGS.DO_NOT_PERSIST;
			if ((allowedCredentialTypes & PSCredentialTypes.Domain) != PSCredentialTypes.Domain)
			{
				dwFlags |= CREDUI_FLAGS.GENERIC_CREDENTIALS;
				if ((options & PSCredentialUIOptions.AlwaysPrompt) == PSCredentialUIOptions.AlwaysPrompt)
				{
					dwFlags |= CREDUI_FLAGS.ALWAYS_SHOW_UI;
				}
			}
			CredUIReturnCodes codes = CredUIReturnCodes.ERROR_INVALID_PARAMETER;
			if ((pszUserName.Length <= 0x201) && (pszPassword.Length <= 0x100))
			{
				codes = CredUIPromptForCredentials(ref structure, targetName, IntPtr.Zero, 0, pszUserName, 0x201, pszPassword, 0x100, ref pfSave, dwFlags);
			}
			if (codes == CredUIReturnCodes.NO_ERROR)
			{
				string str = null;
				if (pszUserName != null)
				{
					str = pszUserName.ToString();
				}
				SecureString password = new SecureString();
				for (int i = 0; i < pszPassword.Length; i++)
				{
					password.AppendChar(pszPassword[i]);
					pszPassword[i] = '\0';
				}
				if (!string.IsNullOrEmpty(str))
				{
					credential = new PSCredential(str, password);
				}
				else
				{
					credential = null;
				}
			}
			else
			{
				if (codes != CredUIReturnCodes.ERROR_CANCELLED)
					throw new OperationCanceledException("CredUIPromptForCredentials returned an error: " + codes.ToString());

				credential = null;
			}
			return credential;
		}
	}
}
