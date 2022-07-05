
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms
{
	/// <summary>
	/// Static text label.
	/// It is created and added to a dialog by <see cref="IDialog.AddText"/>, <see cref="IDialog.AddVerticalText"/>.
	/// </summary>
	public interface IText : IControl
	{
		/// <summary>
		/// Tells to use the same color as for the frame.
		/// </summary>
		bool BoxColor { get; set; }

		/// <include file='doc.xml' path='doc/CenterGroup/*'/>
		bool CenterGroup { get; set; }

		/// <summary>
		/// Tells to draw a single-line (1) or double-line (2) separator including text if any.
		/// </summary>
		int Separator { get; set; }

		/// <include file='doc.xml' path='doc/ShowAmpersand/*'/>
		bool ShowAmpersand { get; set; }

		/// <summary>
		/// Tells to center the text (horizontally or vertically).
		/// </summary>
		/// <remarks>
		/// For separators yet another way to tell this is to use -1 as position in <see cref="IDialog.AddText"/>.
		/// </remarks>
		bool Centered { get; set; }

		/// <summary>
		/// Gets true if the text is vertical.
		/// </summary>
		/// <remarks>
		/// Vertical text controls are added by <see cref="IDialog.AddVerticalText"/>.
		/// </remarks>
		bool Vertical { get; }
	}
}
