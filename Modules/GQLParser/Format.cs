using FarNet;
using GraphQLParser;
using GraphQLParser.Visitors;
using System.IO;
using System.Threading.Tasks;

namespace GQLParser;

/// <summary>
/// GraphQL formatting methods.
/// </summary>
public static class Format
{
	/// <summary>
	/// Formats the cursor file.
	/// </summary>
	/// <param name="path">The optional file path. If it is omitted, the cursor file is used.</param>
	/// <example>
	/// fn: script=GQLParser; method=GQLParser.Format.File
	/// </example>
	public static async Task File(string path)
	{
		// the task starts in the main thread, we can call FarNet
        if (path is null)
        {
            path = Far.Api.FS.CursorFile?.FullName;
            if (path is null)
                return;
        }

		// the rest does not call FarNet, so go full async
        var text1 = System.IO.File.ReadAllText(path);
        var text2 = await FormatAsync(text1);
        await System.IO.File.WriteAllTextAsync(path, text2);
    }

	/// <summary>
	/// Formats the editor selected text.
	/// </summary>
	/// <example>
	/// fn: script=GQLParser; method=GQLParser.Format.Editor
	/// </example>
	public static async Task Editor()
    {
		// the task starts in the main thread, we can call FarNet
		var editor = Far.Api.Editor;
        if (editor is null)
            return;

		// here we call FarNet in the main thread
		var text1 = editor.GetSelectedText();

		// after this async call the thread is unknown, i.e. not main
        var text2 = await FormatAsync(text1);

		// use Tasks.Job to continue in the main thread and use FarNet
		await Tasks.Job(() =>
		{
			editor.BeginUndo();
			editor.SetSelectedText(text2);
			editor.EndUndo();
		});
    }

    private static async Task<string> FormatAsync(string text)
    {
        var document = Parser.Parse(text);
        var writer = new StringWriter();
        var printer = new SDLPrinter();
        await printer.PrintAsync(document, writer);

        if (text.EndsWith('\n'))
            writer.WriteLine();

        return writer.ToString();
    }
}
