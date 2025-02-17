using GraphQLParser;
using GraphQLParser.AST;
using GraphQLParser.Visitors;

namespace XGraphQL;

/// <summary>
/// GraphQL formatting.
/// </summary>
public static class Format
{
	/// <summary>
	/// Formats the specified or cursor file or the current editor text or selection.
	/// </summary>
	/// <param name="path">The optional file path. If it is omitted, the cursor file is used in panels.</param>
	/// <param name="directiveNewLine">Whether to print each directive location on its own line.</param>
	/// <param name="unionMemberNewLine">Whether to print each union member on its own line.</param>
	/// <param name="sort">Tells to sort definitions.</param>
	/// <param name="noEmptyLine">Tells to remove empty lines.</param>
	public static async Task GraphQL(
		string? path,
		bool directiveNewLine,
		bool unionMemberNewLine,
		bool sort,
		bool noEmptyLine)
	{
		var (text1, save) = Abc.GetText(path);

		var text2 = await FormatAsync(
			text1,
			directiveNewLine,
			unionMemberNewLine,
			sort,
			noEmptyLine);

		await save(text2);
	}

	private static byte NodeSortCode(ASTNode node)
	{
		if (node is not INamedNode named)
			return 0;

		if (named.Name.Value == "Query")
			return 1;

		if (named.Name.Value == "Mutation")
			return 2;

		if (named.Name.Value == "Subscription")
			return 3;

		return 4;
	}

	private static async Task<string> FormatAsync(
		string text,
		bool directiveNewLine,
		bool unionMemberNewLine,
		bool sort,
		bool noEmptyLine)
	{
		var document = Parser.Parse(text, new ParserOptions { Ignore = IgnoreOptions.Locations });

		if (sort)
		{
			document.Definitions = [.. document.Definitions
				.OrderBy(NodeSortCode)
				.ThenBy(x => x.Kind)
				.ThenBy(x => x is INamedNode named ? named.Name.StringValue : null)];
		}

		var printer = new SDLPrinter(new SDLPrinterOptions
		{
			PrintComments = true,
			EachDirectiveLocationOnNewLine = directiveNewLine,
			EachUnionMemberOnNewLine = unionMemberNewLine,
		});

		var writer = new StringWriter();
		await printer.PrintAsync(document, writer);

		// remove unwanted end new lines
		var sb = writer.GetStringBuilder();
		if (!text.EndsWith('\n'))
		{
			for (int i = sb.Length - 1; i >= 0 && sb[i] < 32; --i)
				sb.Length = i;
		}

		// remove empty lines
		var result = sb.ToString();
		if (noEmptyLine)
			result = Abc.MyRegexEmptyLines().Replace(result, string.Empty);

		return result;
	}
}
