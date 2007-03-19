using System;

namespace FarManager
{
	/// <summary>
	/// Input box. It is created by <see cref="IFar.CreateInputBox"/>.
	/// </summary>
	public interface IInputBox
	{
		/// <summary>
		/// Text to be edited
		/// </summary>
		string Text { get; set; }
		/// <summary>
		/// title of the box
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Prompt text
		/// </summary>
		string Prompt { get; set; }
		/// <summary>
		/// History string
		/// </summary>
		string History { get; set; }
		/// <summary>
		/// Maximal length of <see cref="Text"/>
		/// </summary>
		int MaxLength { get; set; }
		/// <summary>
		/// Enabled empty input
		/// </summary>
		bool EmptyEnabled { get; set; }
		/// <summary>
		/// Display asterisks instead of input characters
		/// </summary>
		bool IsPassword { get; set; }
		/// <summary>
		/// Expand environment variables
		/// </summary>
		bool EnvExpanded { get; set; }
		/// <summary>
		/// If Text is empty and History is not empty, then do not initialize the input line from the history.
		/// </summary>
		bool NoLastHistory { get; set; }
		/// <summary>
		/// Buttons are visible
		/// </summary>
		bool ButtonsAreVisible { get; set; }
		/// <include file='doc.xml' path='docs/pp[@name="HelpTopic"]/*'/>
		string HelpTopic { get; set; }
		/// <summary>
		/// Shows input box and waits until user press OK or Cancel
		/// </summary>
		/// <returns>true if OK was pressed</returns>
		bool Show();
	}
}
