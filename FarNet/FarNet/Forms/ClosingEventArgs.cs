
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// <see cref="IDialog.Closing"/> event arguments.
/// </summary>
/// <param name="selected">Control that had the keyboard focus when [CtrlEnter] was pressed or the default control.</param>
public sealed class ClosingEventArgs(IControl? selected) : AnyEventArgs(selected)
{
	/// <summary>
	/// Ingore and don't close the dialog.
	/// </summary>
	public bool Ignore { get; set; }
}
