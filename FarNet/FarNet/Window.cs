
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
		/// Commits the results of the last operation on windows (e.g. of <see cref="SetCurrentAt"/>).
		/// </summary>
		/// <returns>true on success.</returns>
		public abstract bool Commit();
		/// <summary>
		/// Sets the current window by the specified window index.
		/// </summary>
		/// <param name="index">Window index. See <see cref="Count"/>.</param>
		/// <remarks>
		/// Window change is actually performed only on <see cref="Commit"/> or when Far gets control.
		/// </remarks>
		public abstract void SetCurrentAt(int index);
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
		/// Returns the window kind name in the current language.
		/// </summary>
		/// <param name="index">
		/// Window index or -1 for the current window.
		/// See <see cref="Count"/>.
		/// </param>
		public abstract string GetKindNameAt(int index);
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
