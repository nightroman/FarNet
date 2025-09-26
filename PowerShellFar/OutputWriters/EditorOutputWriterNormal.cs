using FarNet;

namespace PowerShellFar;

/// <summary>
/// Sync editor writer.
/// </summary>
sealed class EditorOutputWriterNormal(IEditor editor) : EditorOutputWriter(editor)
{
	public override void Flush()
	{
		Editor.Redraw();
	}
}
