using FarNet;

namespace PowerShellFar;

/// <summary>
/// PowerShell breakpoint drawer.
/// </summary>
[ModuleDrawer(Name = "PowerShell breakpoints", Mask = "*.ps1;*.psm1", Priority = 1, Id = "67db13c5-6b7b-4936-b984-e59db08e23c7")]
public class BreakpointDrawer : ModuleDrawer
{
	/// <inheritdoc/>
	public override void Invoke(IEditor editor, ModuleDrawerEventArgs e)
	{
		var fullPath = Path.GetFullPath(editor.FileName); //!
		var colors = new List<EditorColorInfo>();
		bool hasColorer = editor.HasColorer();
		int st = e.Lines[0].Index;
		int en = st + e.Lines.Count;

		foreach (var bp in A.Breakpoints)
		{
			if (!fullPath.Equals(bp.Script, StringComparison.OrdinalIgnoreCase))
				continue;

			int lineIndex = bp.Line - 1;
			if (lineIndex < st || lineIndex >= en)
				continue;

			var line = e.Lines[lineIndex - st];

			var background = bp.Enabled ? ConsoleColor.Yellow : ConsoleColor.Gray;
			if (hasColorer)
			{
				// foreground: keep original but replace yellow and white with black
				// background: yellow
				editor.GetColors(lineIndex, colors);
				foreach (var color in colors)
				{
					e.Colors.Add(new EditorColor(
					lineIndex,
					color.Start,
					color.End,
					(color.Foreground == ConsoleColor.Yellow || color.Foreground == ConsoleColor.White) ? ConsoleColor.Black : color.Foreground,
					background));
				}
			}
			else
			{
				// color all black on yellow
				e.Colors.Add(new EditorColor(
					lineIndex,
					e.StartChar,
					e.EndChar,
					ConsoleColor.Black,
					background));
			}
		}
	}
}
