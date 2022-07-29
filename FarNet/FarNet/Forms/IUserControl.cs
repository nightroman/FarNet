
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Forms;

/// <summary>
/// User control.
/// It is created and added to a dialog by <see cref="IDialog.AddUserControl"/>.
/// </summary>
/// <remarks>
/// Use <see cref="IControl.Drawing"/> event to draw this control by
/// <see cref="IUserInterface.DrawPalette"/> or <see cref="IUserInterface.DrawColor"/>
/// with <see cref="IUserInterface.GetPaletteForeground"/> and <see cref="IUserInterface.GetPaletteBackground"/>.
/// Also usually you should at first calculate absolute coordinates using
/// absolute dialog <see cref="IDialog.Rect"/> and relative control <see cref="IControl.Rect"/>.
/// <para>
/// User control can be used to emulate <c>MouseClicked</c> event for a dialog:
/// add this control so that it covers all the dialog area and use its event handler.
/// </para>
/// </remarks>
public interface IUserControl : IControl
{
	/// <include file='doc.xml' path='doc/NoFocus/*'/>
	bool NoFocus { get; set; }
}
