
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms
{
	/// <summary>
	/// Double line or single line box control.
	/// It is created and added to a dialog by <see cref="IDialog.AddBox"/>.
	/// </summary>
	/// <remarks>
	/// If the box is the first dialog control then its <see cref="IControl.Text"/> is used as the Far console title.
	/// </remarks>
	public interface IBox : IControl
	{
		/// <summary>
		/// Tells to create the single line box.
		/// </summary>
		bool Single { get; set; }

		/// <summary>
		/// Tells to align the text left.
		/// </summary>
		bool LeftText { get; set; }

		/// <include file='doc.xml' path='doc/ShowAmpersand/*'/>
		bool ShowAmpersand { get; set; }
	}
}
