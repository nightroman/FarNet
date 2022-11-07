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
        if (path is null)
        {
            path = Far.Api.FS.CursorFile?.FullName;
            if (path is null)
                return;
        }

        var text1 = System.IO.File.ReadAllText(path);
        var text2 = await FormatAsync(text1);

        System.IO.File.WriteAllText(path, text2);
    }

	/// <summary>
	/// Formats the editor selected text.
	/// </summary>
	/// <example>
	/// fn: script=GQLParser; method=GQLParser.Format.Editor
	/// </example>
	public static async Task Editor()
    {
        var editor = Far.Api.Editor;
        if (editor is null)
            return;

        var text1 = editor.GetSelectedText();
        var text2 = await FormatAsync(text1);

        editor.BeginUndo();
        editor.SetSelectedText(text2);
        editor.EndUndo();
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
