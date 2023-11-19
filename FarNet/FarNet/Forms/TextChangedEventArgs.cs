
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <c>TextChanged</c> event arguments for <see cref="IEdit"/>, <see cref="IComboBox"/>.
/// </summary>
/// <param name="edit">Edit control.</param>
/// <param name="text">New text.</param>
public sealed class TextChangedEventArgs(IControl edit, string text) : AnyEventArgs(edit)
{
	/// <summary>
	/// New text.
	/// </summary>
	public string Text { get; } = text;

	/// <summary>
	/// Ignore changes.
	/// </summary>
	public bool Ignore { get; set; }
}
