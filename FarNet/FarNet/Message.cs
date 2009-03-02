/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

using System;

namespace FarNet
{
	/// <summary>
	/// Message box options.
	/// </summary>
	[Flags]
	public enum MsgOptions
	{
		/// <summary>No options.</summary>
		None,
		/// <summary>Warning message colors are used (white text on red background by default).</summary>
		Warning = 0x00000001,
		/// <summary>If error type returned by GetLastErroris known to FAR or Windows, the error description will be shown in the first message line. In that case, the text given by the plugin will be displayed below the error description.</summary>
		Error = 0x00000002,
		/// <summary>Do not redraw the message background.</summary>
		KeepBackground = 0x00000004,
		/// <summary>Display the message two lines lower than usual.</summary>
		Down = 0x00000008,
		/// <summary>Left align the message lines.</summary>
		LeftAligned = 0x00000010,
		/// <summary>Additional button: Ok.</summary>
		Ok = 0x00010000,
		/// <summary>Additional buttons: Ok and Cancel.</summary>
		OkCancel = 0x00020000,
		/// <summary>Additional buttons: Abort, Retry and Ignore.</summary>
		AbortRetryIgnore = 0x00030000,
		/// <summary>Additional buttons: Yes and No.</summary>
		YesNo = 0x00040000,
		/// <summary>Additional buttons: Yes, No and Cancel.</summary>
		YesNoCancel = 0x00050000,
		/// <summary>Additional buttons: Retry and Cancel.</summary>
		RetryCancel = 0x00060000,
		/// <summary>Reserved.</summary>
		Z1 = 0x00000020,
	}
}
