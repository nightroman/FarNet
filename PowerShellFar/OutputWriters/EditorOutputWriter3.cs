using FarNet;

namespace PowerShellFar;

/// <summary>
/// Async editor writer.
/// </summary>
sealed class EditorOutputWriter3(IEditor editor) : EditorOutputWriter1(editor)
{
	protected override void Redraw()
	{
		Far.Api.PostJob(() =>
		{
			Editor.Sync();
			Editor.Redraw();
		});
	}
}
