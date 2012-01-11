
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

namespace FarNet
{
	/// <summary>
	/// Far windows operator.
	/// </summary>
	public abstract class IWindow
	{
		/// <summary>
		/// Gets open window count.
		/// </summary>
		/// <remarks>
		/// There is at least one window (panels, editor, or viewer).
		/// </remarks>
		public abstract int Count { get; }
		/// <summary>
		/// Gets the current window kind.
		/// </summary>
		/// <remarks>
		/// It is the same as the result of <see cref="GetKindAt"/> with the index -1.
		/// </remarks>
		public abstract WindowKind Kind { get; }
		/// <summary>
		/// Returns the window kind.
		/// </summary>
		/// <param name="index">
		/// Window index or -1 for the current window, same as <see cref="WindowKind"/>.
		/// See <see cref="Count"/>.
		/// </param>
		public abstract WindowKind GetKindAt(int index);
		/// <summary>
		/// Returns the window title.
		/// </summary>
		/// <param name="index">
		/// Window index or -1 for the current window.
		/// See <see cref="Count"/>.
		/// </param>
		/// <remarks>
		/// Window title:
		/// viewer, editor: the file name;
		/// panels: selected file name;
		/// help: .hlf file path;
		/// menu, dialog: header.
		/// </remarks>
		public abstract string GetNameAt(int index);
		/// <summary>
		/// Sets the current window by the specified index.
		/// </summary>
		/// <param name="index">Window index. See <see cref="Count"/>.</param>
		public abstract void SetCurrentAt(int index);
	}

	/// <summary>
	/// Window kind constants.
	/// </summary>
	public enum WindowKind
	{
		///
		None,
		/// <summary>
		/// File panels.
		/// </summary>
		Panels = 1,
		/// <summary>
		/// Internal viewer window.
		/// </summary>
		Viewer = 2,
		/// <summary>
		/// Internal editor window.
		/// </summary>
		Editor = 3,
		/// <summary>
		/// Dialog.
		/// </summary>
		Dialog = 4,
		/// <summary>
		/// Menu.
		/// </summary>
		Menu = 5,
		/// <summary>
		/// Help window.
		/// </summary>
		Help = 6
	}
}
