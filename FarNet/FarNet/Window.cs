/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
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
		/// Returns information about the window spesified by its index.
		/// </summary>
		/// <param name="index">Window index; -1 ~ current. See <see cref="Count"/>.</param>
		/// <param name="full">
		/// If it is false then <see cref="IWindowInfo.Name"/> and <see cref="IWindowInfo.KindName"/> are not filled.
		/// </param>
		public abstract IWindowInfo GetInfoAt(int index, bool full);
		/// <summary>
		/// Returns a type of a window specified by the index.
		/// </summary>
		/// <param name="index">
		/// Window index or -1 for the current window, same as <see cref="WindowKind"/>.
		/// See <see cref="Count"/>.
		/// </param>
		public abstract WindowKind GetKindAt(int index);
	}

	/// <summary>
	/// Far window kinds.
	/// </summary>
	public enum WindowKind
	{
		/// <summary>
		/// Dummy.
		/// </summary>
		None,
		/// <summary>
		/// File panels.
		/// </summary>
		Panels,
		/// <summary>
		/// Internal viewer window.
		/// </summary>
		Viewer,
		/// <summary>
		/// Internal editor window.
		/// </summary>
		Editor,
		/// <summary>
		/// Dialog.
		/// </summary>
		Dialog,
		/// <summary>
		/// Menu.
		/// </summary>
		Menu,
		/// <summary>
		/// Help window.
		/// </summary>
		Help
	}

	/// <summary>
	/// Contains information about one Far window. See <see cref="IWindow.GetInfoAt"/>.
	/// </summary>
	public interface IWindowInfo
	{
		/// <summary>
		/// Window kind.
		/// </summary>
		WindowKind Kind { get; }
		/// <summary>
		/// Modification flag. Valid only for editor window.
		/// </summary>
		bool Modified { get; }
		/// <summary>
		/// Is the window active?
		/// </summary>
		bool Current { get; }
		/// <summary>
		/// Name of the window kind depending on the current Far language.
		/// </summary>
		string KindName { get; }
		/// <summary>
		/// Window title:
		/// viewer, editor: the file name;
		/// panels: selected file name;
		/// help: .hlf file path;
		/// menu, dialog: header.
		/// </summary>
		string Name { get; }
	}

}
