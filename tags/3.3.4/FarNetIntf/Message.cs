using System;
using System.Collections.Specialized;

namespace FarManager
{
	/// <summary>
	/// Message box
	/// </summary>
	public interface IMessage
	{
		/// <summary>
		/// Body of message
		/// </summary>
		StringCollection Body { get; }
		/// <summary>
		/// Buttons of message box
		/// </summary>
		StringCollection Buttons { get; }
		/// <summary>
		/// Header of message box
		/// </summary>
		String Header { get; set; }
		/// <summary>
		///  Index of selected <see cref="Buttons">button</see>
		/// </summary>
		int Selected { get; set; }
		/// <summary>
		/// Message is warning
		/// </summary>
		bool IsWarning { get; set; }
		/// <summary>
		/// Message is error 
		/// </summary>
		bool IsError { get; set; }
		/// <summary>
		/// Store and restore background contents
		/// </summary>
		bool KeepBackground { get; set; }
		/// <summary>
		/// Align message text to the left
		/// </summary>
		bool LeftAligned { get; set; }
		/// <summary>
		/// Describes a help topic. <see cref="IFar.ShowHelp"/> for details.
		/// </summary>
		string HelpTopic { get; set; }
		/// <summary>
		/// Show the message box
		/// </summary>
		/// <returns>Ok is pressed</returns>
		bool Show();
		/// <summary>
		/// Reset all properties to initial values
		/// </summary>
		void Reset();
		/// <summary>
		/// Message options.
		/// </summary>
		MessageOptions Options { get; set; }
	}

	/// <summary>
	/// Message box options for <see cref="IFar.Msg(string,string,MessageOptions)"/>.
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
		/// <summary>Don't use it.</summary>
		Reserved1 = 0x00000020,
	}
}
