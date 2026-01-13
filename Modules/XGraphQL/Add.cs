using FarNet;
using GraphQLParser;
using GraphQLParser.AST;
using GraphQLParser.Visitors;
using System.Collections.Frozen;

namespace XGraphQL;

/// <summary>
/// Adding GraphQL to editor.
/// </summary>
public static class Add
{
	static readonly FrozenSet<string> _definedNames = ["Int", "Float", "String", "Boolean", "ID"];

	/// <summary>
	/// Adds undefined referenced types as scalars.
	/// </summary>
	public static async Task UndefinedTypes()
	{
		// call FarNet before await
		var editor = Abc.GetEditor();
		var text = editor.GetText();

		var document = Parser.Parse(text, new ParserOptions { Ignore = IgnoreOptions.Locations });

		// get definitions
		HashSet<string> definitionNames = [];
		foreach (var node in document.Definitions)
		{
			if (node is INamedNode namedNode)
				definitionNames.Add(namedNode.Name.StringValue);
		}

		// get references
		var namesContext = new NamesContext();
		await new NamesVisitor().VisitAsync(document, namesContext);

		// undefined references
		var undefinedNames = namesContext.Names.Except(_definedNames).Except(definitionNames).ToList();
		if (undefinedNames.Count == 0)
			return;

		undefinedNames.Sort();

		// call FarNet after await
		await Far.Api.PostJobAsync(() =>
		{
			editor.BeginUndo();
			try
			{
				editor.GoToEnd(true);
				var strings = editor.Strings;
				foreach (var name in undefinedNames)
					strings.Add($"scalar {name}");
			}
			finally
			{
				editor.EndUndo();
			}
		});
	}

	class NamesContext : IASTVisitorContext
	{
		public HashSet<string> Names { get; } = [];
		public CancellationToken CancellationToken { get; }
	}

	class NamesVisitor : ASTVisitor<NamesContext>
	{
		public override async ValueTask VisitAsync(ASTNode? node, NamesContext context)
		{
			if (node is null)
				return;

			if (node is GraphQLNamedType namedType)
				context.Names.Add(namedType.Name.StringValue);

			await base.VisitAsync(node, context);
		}
	}
}
