
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;

namespace PowerShellFar;

/// <summary>
/// Asynchronous editor writer with the async mode editor.
/// </summary>
sealed class EditorOutputWriter3 : EditorOutputWriter1
{
	public EditorOutputWriter3(IEditor editor) : base(editor)
	{
	}

	protected override void Redraw()
	{
		Far.Api.PostJob(Editor.Sync);
	}
}
