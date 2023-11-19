
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Forms;

/// <summary>
/// <see cref="IControl.Coloring"/> event arguments.
/// </summary>
/// <remarks>
/// Event handlers change the default colors provided by the event arguments.
/// There are up to 4 color pairs (foreground and background).
/// <para>
/// <see cref="IBox"/>: 1: Title; 2: HiText; 3: Frame.
/// </para>
/// <para>
/// <see cref="IText"/>:
/// Normal text: 1: Title; 2: HiText; 3: Frame.
/// Vertical text: 1: Title.
/// The box color applies only to text items with the <see cref="IText.Separator"/> flag set.
/// </para>
/// <para>
/// <see cref="IEdit"/>, <see cref="IComboBox"/>: 1: EditLine; 2: Selected Text; 3: Unchanged Color; 4: History and ComboBox pointer.
/// </para>
/// <para>
/// <see cref="IButton"/>, <see cref="ICheckBox"/>, <see cref="IRadioButton"/>: 1: Title; 2: HiText.
/// </para>
/// <para>
/// <see cref="IListBox"/> recieves another event which is not yet exposed by FarNet.
/// </para>
/// </remarks>
/// <param name="control">Control for setting colors.</param>
public sealed class ColoringEventArgs(IControl control) : AnyEventArgs(control)
{
	/// <summary>
	/// Color 1, foreground.
	/// </summary>
	public ConsoleColor Foreground1 { get; set; }

	/// <summary>
	/// Color 1, background.
	/// </summary>
	public ConsoleColor Background1 { get; set; }

	/// <summary>
	/// Color 2, foreground.
	/// </summary>
	public ConsoleColor Foreground2 { get; set; }

	/// <summary>
	/// Color 2, background.
	/// </summary>
	public ConsoleColor Background2 { get; set; }

	/// <summary>
	/// Color 3, foreground.
	/// </summary>
	public ConsoleColor Foreground3 { get; set; }

	/// <summary>
	/// Color 3, background.
	/// </summary>
	public ConsoleColor Background3 { get; set; }

	/// <summary>
	/// Color 4, foreground.
	/// </summary>
	public ConsoleColor Foreground4 { get; set; }

	/// <summary>
	/// Color 4, background.
	/// </summary>
	public ConsoleColor Background4 { get; set; }
}
