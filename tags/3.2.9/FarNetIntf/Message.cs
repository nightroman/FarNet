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
		/// Show the message box
		/// </summary>
		/// <returns>Ok is pressed</returns>
		bool Show();
		/// <summary>
		/// Reset all properties to initial values
		/// </summary>
		void Reset();
	}
}
