using FarNet;
using Json.More;
using Json.Path;
using JsonKit.About;
using JsonKit.Panels;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonKit.Commands;

sealed class OpenCommand(CommandParameters parameters) : AnyCommand
{
	readonly string? _file = parameters.GetString(Host.Param.File, expandVariables: true, resolveFullPath: true);
	readonly string? _select = parameters.GetString(Host.Param.Select);

	public override void Invoke()
	{
		var file = _file ?? Far.Api.FS.CursorFile?.FullName;
		if (file is null)
		{
			if (_select is { } && Far.Api.Panel is AbcPanel panel)
			{
				CreateArrayExplorer(panel.MyExplorer.JsonNode, _select).OpenPanelChild(panel);
				return;
			}

			throw new ModuleException("The panel cursor should be a file.");
		}

		var bytes = File.ReadAllBytes(file);
		var index = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
		var span = bytes.AsSpan(index);

		JsonReaderOptions options = new() { AllowMultipleValues = true, AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
		Utf8JsonReader reader = new(span, options);

		var nodes = JsonNodeReader.ReadNodes(ref reader);

		AbcExplorer explorer;
		if (nodes.Count == 1 && nodes[0] is JsonObject jsonObject)
		{
			if (_select is null)
			{
				explorer = new ObjectExplorer(jsonObject, new() { FilePath = _file });
			}
			else
			{
				explorer = CreateArrayExplorer(jsonObject, _select);
			}
		}
		else
		{
			if (nodes.Count != 1 || nodes[0] is not JsonArray jsonArray)
				jsonArray = new JsonArray([.. nodes]);

			if (_select is null)
			{
				explorer = new ArrayExplorer(jsonArray, new() { FilePath = _file });
			}
			else
			{
				explorer = CreateArrayExplorer(jsonArray, _select);
			}
		}

		explorer.OpenPanel();
	}

	static ArrayExplorer CreateArrayExplorer(JsonNode root, string select)
	{
		var jsonPath = JsonPath.Parse(select);
		var res = jsonPath.Evaluate(root);
		var jsonArray = res.Matches.Select(x => x.Value).ToJsonArray();
		return new ArrayExplorer(jsonArray, new());
	}
}
