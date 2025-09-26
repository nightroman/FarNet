using FarNet;

namespace PowerShellFar;

/// <summary>
/// Async editor writer.
/// </summary>
sealed class EditorOutputWriterAsync(IEditor editor) : EditorOutputWriter(editor)
{
	public override void Flush()
	{
		A.AwaitJob(() =>
		{
			Editor.Sync();
			Editor.Redraw();
		});
	}
}
