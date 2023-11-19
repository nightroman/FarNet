
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <c>ButtonClicked</c> event arguments for <see cref="IButton"/>, <see cref="ICheckBox"/>, <see cref="IRadioButton"/>.
/// </summary>
/// <param name="button">Button clicked.</param>
/// <param name="selected">Selected state.</param>
public sealed class ButtonClickedEventArgs(IControl button, int selected) : AnyEventArgs(button)
{
	/// <summary>
	/// Selected state:
	/// <see cref="IButton"/>: 0;
	/// <see cref="ICheckBox"/>: 0 (unchecked), 1 (checked) and 2 (undefined for ThreeState);
	/// <see cref="IRadioButton"/>: 0 - for the previous element in the group, 1 - for the active element in the group.
	/// </summary>
	public int Selected { get; } = selected;

	/// <summary>
	/// The message has been handled and it should not be processed by the kernel.
	/// </summary>
	public bool Ignore { get; set; }
}
