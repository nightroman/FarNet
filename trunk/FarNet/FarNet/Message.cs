
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;

namespace FarNet
{
	/// <summary>
	/// Parameters of <see cref="IFar.Message(MessageArgs)"/>.
	/// </summary>
	public class MessageArgs
	{
		/// <summary>
		/// Message text.
		/// </summary>
		public string Text { get; set; }
		/// <summary>
		/// Message caption.
		/// </summary>
		public string Caption { get; set; }
		/// <summary>
		/// Message options.
		/// </summary>
		public MessageOptions Options { get; set; }
		/// <summary>
		/// Message buttons. Not supported with <c>Gui*</c> options.
		/// </summary>
		public string[] Buttons { get; set; }
		/// <summary>
		/// <include file='doc.xml' path='doc/HelpTopic/*'/>
		/// It is ignored in GUI and drawn messages.
		/// </summary>
		public string HelpTopic { get; set; }
		/// <summary>
		/// Message position.
		/// </summary>
		public Point? Position { get; set; }
	}
	/// <summary>
	/// Message box options.
	/// </summary>
	[Flags]
	public enum MessageOptions
	{
		/// <summary>
		/// No options.
		/// </summary>
		None,
		/// <summary>
		/// Warning message colors are used (white text on red background by default).
		/// </summary>
		Warning = 0x1,
		/// <summary>
		/// If error type returned by <c>GetLastError</c> is known to Far or Windows,
		/// the error description is shown in the first message line and
		/// the text is shown below the error description.
		/// </summary>
		Error = 0x2,
		/// <summary>
		/// Do not redraw the message background.
		/// </summary>
		KeepBackground = 0x4,
		/// <summary>
		/// Left align the message lines.
		/// </summary>
		LeftAligned = 0x8,
		/// <summary>
		/// Reserved.
		/// </summary>
		Z1 = 0x10,
		/// <summary>
		/// Additional button: Ok.
		/// </summary>
		Ok = 0x10000,
		/// <summary>
		/// Additional buttons: Ok and Cancel.
		/// </summary>
		OkCancel = 0x20000,
		/// <summary>
		/// Additional buttons: Abort, Retry and Ignore.
		/// </summary>
		AbortRetryIgnore = 0x30000,
		/// <summary>
		/// Additional buttons: Yes and No.
		/// </summary>
		YesNo = 0x40000,
		/// <summary>
		/// Additional buttons: Yes, No and Cancel.
		/// </summary>
		YesNoCancel = 0x50000,
		/// <summary>
		/// Additional buttons: Retry and Cancel.
		/// </summary>
		RetryCancel = 0x60000,
		/// <summary>
		/// For internal use (normally). GUI message.
		/// </summary>
		Gui = 0x00000040,
		/// <summary>
		/// For internal use (normally). GUI message on a macro in progress.
		/// </summary>
		GuiOnMacro = 0x00000080,
		/// <summary>
		/// Tells to draw the message box with no buttons and continue.
		/// </summary>
		Draw = 0x00000100,
	}
}
