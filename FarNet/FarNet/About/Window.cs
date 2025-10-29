namespace FarNet;

/// <summary>
/// Far windows operator.
/// </summary>
public abstract class IWindow
{
	/// <summary>
	/// Gets the window count.
	/// </summary>
	/// <remarks>
	/// There is at least one window (panels, editor, or viewer).
	/// </remarks>
	public abstract int Count { get; }

	/// <summary>
	/// Gets the current window kind.
	/// Any thread may call this.
	/// </summary>
	/// <remarks>
	/// The result is the same as <see cref="GetKindAt"/> with -1.
	/// </remarks>
	public abstract WindowKind Kind { get; }

	/// <summary>
	/// Gets true if the the current window is modal.
	/// </summary>
	public abstract bool IsModal { get; }

	/// <summary>
	/// Gets the window interface.
	/// </summary>
	/// <param name="index">Window index or -1 for the current window.</param>
	public abstract IFace? GetAt(int index);

	/// <summary>
	/// Gets the window kind.
	/// </summary>
	/// <param name="index">Window index or -1 for the current window.</param>
	public abstract WindowKind GetKindAt(int index);

	/// <summary>
	/// Gets the window title.
	/// </summary>
	/// <param name="index">Window index or -1 for the current window.</param>
	/// <remarks>
	/// Window title:
	/// viewer, editor: the file name;
	/// panels: selected file name;
	/// help: HLF file path;
	/// menu, dialog: header.
	/// </remarks>
	public abstract string GetNameAt(int index);

	/// <summary>
	/// Gets the windo identifier.
	/// </summary>
	/// <param name="index">Window index or -1 for the current window.</param>
	public abstract nint GetIdAt(int index);

	/// <summary>
	/// Sets the current window by the specified index.
	/// </summary>
	/// <param name="index">Window index or -1 for the current window.</param>
	public abstract void SetCurrentAt(int index);
}
