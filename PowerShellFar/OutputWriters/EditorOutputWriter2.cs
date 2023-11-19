
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;

namespace PowerShellFar;

/// <summary>
/// Synchronous editor writer.
/// </summary>
sealed class EditorOutputWriter2(IEditor editor) : EditorOutputWriter1(editor)
{
	protected override void Redraw()
	{
		Editor.Redraw();
	}
}
