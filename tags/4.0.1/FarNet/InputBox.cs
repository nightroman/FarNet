/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

using System;

namespace FarNet
{
	/// <summary>
	/// Input box. It is created by <see cref="IFar.CreateInputBox"/>.
	/// </summary>
	public interface IInputBox
	{
		/// <summary>
		/// Text to be edited.
		/// </summary>
		string Text { get; set; }
		/// <summary>
		/// Title of the box.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Prompt text.
		/// </summary>
		string Prompt { get; set; }
		/// <summary>
		/// History string.
		/// </summary>
		string History { get; set; }
		/// <summary>
		/// Maximal length of <see cref="Text"/>.
		/// </summary>
		int MaxLength { get; set; }
		/// <summary>
		/// Enabled empty input.
		/// </summary>
		bool EmptyEnabled { get; set; }
		/// <summary>
		/// Display asterisks instead of input characters.
		/// </summary>
		bool IsPassword { get; set; }
		/// <summary>
		/// Expand environment variables.
		/// </summary>
		bool EnvExpanded { get; set; }
		/// <summary>
		/// If <see cref="Text"/> is empty and <see cref="History"/> is not empty,
		/// then do not initialize the input line from the history.
		/// </summary>
		bool NoLastHistory { get; set; }
		/// <summary>
		/// Buttons are visible.
		/// </summary>
		bool ButtonsAreVisible { get; set; }
		/// <summary>
		/// The only supported format is "&lt;FullPath\&gt;Topic", see <see cref="IFar.ShowHelp"/>.
		/// </summary>
		string HelpTopic { get; set; }
		/// <summary>
		/// Shows input box and waits until user press OK or Cancel.
		/// </summary>
		/// <returns>True if OK was pressed.</returns>
		bool Show();
	}
}
