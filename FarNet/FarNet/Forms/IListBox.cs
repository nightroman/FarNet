
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms
{
	/// <summary>
	/// List box control.
	/// It is created and added to a dialog by <see cref="IDialog.AddListBox"/>.
	/// </summary>
	public interface IListBox : IBaseList
	{
		/// <summary>
		/// Tells to not draw the box around the list.
		/// </summary>
		bool NoBox { get; set; }

		/// <summary>
		/// Gets or sets the title line text.
		/// </summary>
		string Title { get; set; }

		/// <summary>
		/// Gets or sets the bottom line text.
		/// </summary>
		string Bottom { get; set; }

		/// <summary>
		/// Sets both cursor and top positions. It should be called when a dialog is shown.
		/// </summary>
		/// <param name="selected">Index of the selected item.</param>
		/// <param name="top">Index of the top item.</param>
		void SetFrame(int selected, int top);
	}
}
