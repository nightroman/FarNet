using FarNet;
using System.Text.RegularExpressions;

namespace XGraphQL;

static partial class Abc
{
	[GeneratedRegex(@"(?m:^\s*(?:\r?\n|\r)+)")]
	public static partial Regex MyRegexEmptyLines();

	/// <summary>
	/// Gets the existing editor or fails.
	/// </summary>
	public static IEditor GetEditor() =>
		Far.Api.Editor ?? throw new ModuleException("Expected editor.");

	/// <summary>
	/// Gets text and its setter from a source:
	/// (1) file with the specified path
	/// (2) in panels, the cursor file
	/// (3) in editor, selected text
	/// (4) in editor, all text
	/// </summary>
	/// <param name="path">The optional explicit file to use.</param>
	public static (string, Func<string, Task> save) GetText(string? path)
	{
		if (path is null)
		{
			switch (Far.Api.Window.Kind)
			{
				case WindowKind.Panels:
					{
						path = Far.Api.FS.CursorFile?.FullName;
						if (path is null)
							throw new ModuleException("Cannot get text from file.");
					}
					break;
				case WindowKind.Editor:
					{
						var editor = Far.Api.Editor!;
						var text = editor.GetSelectedText();
						if (text.Length > 0)
							return (text, text => SetEditorTextAsync(editor, text, true));
						else
							return (editor.GetText(), text => SetEditorTextAsync(editor, text, false));
					}
				default:
					{
						throw new ModuleException("Cannot get text from file or editor.");
					}
			}
		}
		else
		{
			path = Far.Api.GetFullPath(Environment.ExpandEnvironmentVariables(path));
		}

		var text1 = File.ReadAllText(path);
		return (text1, text => WriteAllTextAsync(path, text));
	}

	private static async Task WriteAllTextAsync(string path, string text)
	{
		await File.WriteAllTextAsync(path, text);
	}

	private static async Task SetEditorTextAsync(IEditor editor, string text, bool selected)
	{
		await Far.Api.PostJobAsync(() =>
		{
			editor.BeginUndo();
			try
			{
				if (selected)
					editor.SetSelectedText(text);
				else
					editor.SetText(text);
			}
			finally
			{
				editor.EndUndo();
			}
		});
	}
}
