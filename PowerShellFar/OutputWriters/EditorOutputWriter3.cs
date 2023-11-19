
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;

namespace PowerShellFar;

/// <summary>
/// Asynchronous editor writer with the async mode editor.
/// </summary>
sealed class EditorOutputWriter3(IEditor editor) : EditorOutputWriter1(editor)
{
	protected override void Redraw()
	{
		Far.Api.PostJob(Editor.Sync);
	}
}
