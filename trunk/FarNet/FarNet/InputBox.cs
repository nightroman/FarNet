
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

namespace FarNet
{
	/// <summary>
	/// Input box. It is created by <see cref="IFar.CreateInputBox"/>.
	/// </summary>
	public interface IInputBox
	{
		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		string Text { get; set; }
		/// <summary>
		/// Gets or sets the box title.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Gets or sets the prompt text.
		/// </summary>
		string Prompt { get; set; }
		/// <summary>
		/// Gets or sets the history name.
		/// </summary>
		string History { get; set; }
		/// <summary>
		/// Gets or sets the max text length.
		/// </summary>
		int MaxLength { get; set; }
		/// <summary>
		/// Tells to enable empty input permission.
		/// </summary>
		bool EmptyEnabled { get; set; }
		/// <summary>
		/// Tells that it is used for file system path input.
		/// </summary>
		/// <remarks>
		/// Setting this to true enables some extras, e.g. on typing: a dropdown list of matching available paths.
		/// </remarks>
		bool IsPath { get; set; }
		/// <summary>
		/// Tells that it is used for password input.
		/// </summary>
		/// <remarks>
		/// If it is true then asterisks are displaied instead of input characters.
		/// </remarks>
		bool IsPassword { get; set; }
		/// <include file='doc.xml' path='doc/ExpandEnvironmentVariables/*'/>
		bool ExpandEnvironmentVariables { get; set; }
		/// <include file='doc.xml' path='doc/UseLastHistory/*'/>
		bool UseLastHistory { get; set; }
		/// <summary>
		/// Tells that buttons are visible.
		/// </summary>
		bool ButtonsAreVisible { get; set; }
		/// <summary>
		/// Gets or sets the help topic; the only supported format is "&lt;FullPath\&gt;Topic", see <see cref="IFar.ShowHelp"/>.
		/// </summary>
		string HelpTopic { get; set; }
		/// <summary>
		/// Shows the input box and waits until user press OK or Cancel.
		/// </summary>
		/// <returns>True if OK is pressed.</returns>
		bool Show();
	}
}
