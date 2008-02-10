/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

using System;
using System.Collections.Specialized;

namespace FarManager
{
	/// <summary>
	/// Message box. It is created by <see cref="IFar.CreateMessage"/>.
	/// In many cases you may just use one of <see cref="IFar.Msg(string)"/> methods.
	/// </summary>
	public interface IMessage
	{
		/// <summary>
		/// Message body: lines of text.
		/// </summary>
		StringCollection Body { get; }
		/// <summary>
		/// Set of button labels.
		/// </summary>
		StringCollection Buttons { get; }
		/// <summary>
		/// Message box header.
		/// </summary>
		String Header { get; set; }
		/// <summary>
		/// Index of a selected button or -1 on Escape.
		/// </summary>
		int Selected { get; set; }
		/// <summary>
		/// Message is warning.
		/// </summary>
		bool IsWarning { get; set; }
		/// <summary>
		/// Message is error.
		/// </summary>
		bool IsError { get; set; }
		/// <summary>
		/// Store and restore background contents.
		/// </summary>
		bool KeepBackground { get; set; }
		/// <summary>
		/// Align message text to the left.
		/// </summary>
		bool LeftAligned { get; set; }
		/// <include file='doc.xml' path='docs/pp[@name="HelpTopic"]/*'/>
		string HelpTopic { get; set; }
		/// <summary>
		/// Show the message box.
		/// </summary>
		/// <returns>True if a button is pressed.</returns>
		bool Show();
		/// <summary>
		/// Reset all properties to initial values.
		/// </summary>
		void Reset();
		/// <summary>
		/// Message options.
		/// </summary>
		MessageOptions Options { get; set; }
	}

	/// <summary>
	/// Message box options.
	/// </summary>
	[Flags]
	public enum MessageOptions
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
		/// <summary>Left align the message lines instead of centering them.</summary>
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
		Reserved1 = 0x00000020,
	}
}
