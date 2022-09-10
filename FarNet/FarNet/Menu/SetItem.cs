
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Item of a menu, a list menu or one of list dialog controls.
/// </summary>
/// <seealso cref="IMenu"/>
/// <seealso cref="IListMenu"/>
/// <seealso cref="Forms.IListBox"/>
/// <seealso cref="Forms.IComboBox"/>
public sealed class SetItem : FarItem
{
	/// <inheritdoc/>
	public override string Text
	{
		get => _text ?? string.Empty;
		set => _text = value;
	}
	string? _text;

	/// <inheritdoc/>
	public override bool Checked { get; set; }

	/// <inheritdoc/>
	public override bool Disabled { get; set; }

	/// <inheritdoc/>
	public override bool Grayed { get; set; }

	/// <inheritdoc/>
	public override bool Hidden { get; set; }

	/// <inheritdoc/>
	public override bool IsSeparator { get; set; }

	/// <inheritdoc/>
	public override object? Data { get; set; }

	/// <inheritdoc/>
	public override EventHandler<MenuEventArgs>? Click { get; set; }
}
