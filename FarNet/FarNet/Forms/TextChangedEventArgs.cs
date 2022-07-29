
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <c>TextChanged</c> event arguments for <see cref="IEdit"/>, <see cref="IComboBox"/>.
/// </summary>
public sealed class TextChangedEventArgs : AnyEventArgs
{
	/// <param name="edit">Edit control.</param>
	/// <param name="text">New text.</param>
	public TextChangedEventArgs(IControl edit, string text) : base(edit)
	{
		Text = text;
	}

	/// <summary>
	/// New text.
	/// </summary>
	public string Text { get; }

	/// <summary>
	/// Ignore changes.
	/// </summary>
	public bool Ignore { get; set; }
}
