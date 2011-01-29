/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Show-FarMsg command.
	/// Shows a message box.
	/// </summary>
	/// <remarks>
	/// If there are two or more buttons returns selected button index or -1 on escape,
	/// otherwise nothing is returned, it is used just to display a message.
	/// </remarks>
	/// <seealso cref="IFar.Message(string)"/>
	[Description("Shows a message box.")]
	public sealed class ShowFarMessageCommand : BaseCmdlet
	{
		/// <summary>
		/// Message text. Text with long lines or many lines is allowed, but some lines may be not shown.
		/// </summary>
		[Parameter(Position = 0, Mandatory = true, HelpMessage = "Message text. Text with long lines or many lines is allowed, but some lines may be not shown.")]
		[AllowEmptyString]
		[AllowNull]
		public string Text { get; set; }

		/// <summary>
		/// Message caption.
		/// </summary>
		[Parameter(Position = 1, HelpMessage = "Message caption.")]
		public string Caption { get; set; }

		/// <summary>
		/// Standard message buttons.
		/// </summary>
		[Parameter(Position = 2, HelpMessage = "Standard message buttons.")]
		public ButtonSet Buttons { get; set; }

		/// <summary>
		/// User defined choice buttons. On too many choices a message box internally may be converted into a dialog.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[Parameter(HelpMessage = "User defined choice buttons. On too many choices a message box internally may be converted into a dialog.")]
		public string[] Choices { get; set; }

		/// <summary>
		/// Help topic.
		/// </summary>
		[Parameter(HelpMessage = "Help topic.")]
		public string HelpTopic { get; set; }

		/// <summary>
		/// Tells to draw the message box with no buttons and continue. The caller has to redraw or restore the screen.
		/// </summary>
		[Parameter(HelpMessage = "Tells to draw the message box with no buttons and continue. The caller has to redraw or restore the screen.")]
		public SwitchParameter Draw { get; set; }

		/// <summary>
		/// Left align the message lines.
		/// </summary>
		[Parameter(HelpMessage = "Left align the message lines.")]
		public SwitchParameter LeftAligned { get; set; }

		/// <summary>
		/// Do not redraw the message background.
		/// </summary>
		[Parameter(HelpMessage = "Do not redraw the message background.")]
		public SwitchParameter KeepBackground { get; set; }

		/// <summary>
		/// If error type returned by GetLastError is known to Far or Windows, the error description will be shown before the message body text.
		/// </summary>
		[Parameter(HelpMessage = "If error type returned by GetLastError is known to Far or Windows, the error description will be shown before the message body text.")]
		public SwitchParameter IsError { get; set; }

		/// <summary>
		/// Warning message colors are used (white text on red background by default).
		/// </summary>
		[Parameter(HelpMessage = "Warning message colors are used (white text on red background by default).")]
		public SwitchParameter IsWarning { get; set; }

		///
		protected override void BeginProcessing()
		{
			if (Buttons != ButtonSet.Ok && Choices != null && Choices.Length > 0)
				throw new RuntimeException("Parameters 'Buttons' and 'Choices' cannot be used together.");

			MsgOptions options;
			if (Draw)
			{
				options = MsgOptions.Draw;
			}
			else
			{
				options = MsgOptions.None;
				switch (Buttons)
				{
					case ButtonSet.AbortRetryIgnore: options |= MsgOptions.AbortRetryIgnore; break;
					case ButtonSet.OkCancel: options |= MsgOptions.OkCancel; break;
					case ButtonSet.RetryCancel: options |= MsgOptions.RetryCancel; break;
					case ButtonSet.YesNo: options |= MsgOptions.YesNo; break;
					case ButtonSet.YesNoCancel: options |= MsgOptions.YesNoCancel; break;
				}
			}

			if (KeepBackground)
				options |= MsgOptions.KeepBackground;
			if (IsError)
				options |= MsgOptions.Error;
			if (IsWarning)
				options |= MsgOptions.Warning;
			if (LeftAligned)
				options |= MsgOptions.LeftAligned;

			int r = Far.Net.Message(Text ?? string.Empty, Caption, options, Choices, HelpTopic);
			if (!Draw && (Buttons != ButtonSet.Ok || Choices != null && Choices.Length > 0))
				WriteObject(r);
		}
	}
}
