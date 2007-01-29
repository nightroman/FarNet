using System;

namespace FarManager
{
	/// <summary>
	/// Input box
	/// </summary>
	/// <seealso cref="IFar.CreateInputBox"/>
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
		/// Use last history as default value
		/// </summary>
		bool UseLastHistory { get; set; }
		/// <summary>
		/// Buttons are visible
		/// </summary>
		bool ButtonsAreVisible { get; set; }
		/// <summary>
		/// Show message box
		/// </summary>
		/// <param name="prompt">parameter overrides property with the same name</param>
		/// <param name="text">parameter overrides property with the same name</param>
		/// <param name="title">parameter overrides property with the same name</param>
		/// <param name="history">parameter overrides property with the same name</param>
		/// <returns>true if OK was pressed</returns>
		bool Show(string prompt, string text, string title, string history);
		/// <summary>
		/// Show list boz and wait until user press OK or Cancel
		/// </summary>
		/// <returns>true if OK was pressed</returns>
		bool Show();
	}
}
