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

sealed class OpenCommand(CommandParameters parameters) : AbcCommand
{
	readonly string? _file = parameters.GetPath(Param.File, ParameterOptions.UseCursorFile);
	readonly string? _select = parameters.GetString(Param.Select);

	public override void Invoke()
	{
		if (_file is null)
		{
			if (_select is { } && Far.Api.Panel is AbcPanel panel)
			{
				CreateAbcExplorer(panel.MyExplorer.JsonNode, _select).OpenPanelChild(panel);
				return;
			}

			throw parameters.ParameterError(Param.File, "Omitted requires the panel cursor file.");
		}

		var bytes = File.ReadAllBytes(_file);
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
				explorer = CreateAbcExplorer(jsonObject, _select);
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
				explorer = CreateAbcExplorer(jsonArray, _select);
			}
		}

		explorer.OpenPanel();
	}

	static AbcExplorer CreateAbcExplorer(JsonNode root, string select)
	{
		var jsonPath = JsonPath.Parse(select);
		var res = jsonPath.Evaluate(root);

		//: singular array or object
		if (jsonPath.IsSingular && res.Matches.Count == 1)
		{
			var node = res.Matches[0].Value;
			if (node is JsonArray jsonArray)
				return new ArrayExplorer(jsonArray, new());
			if (node is JsonObject jsonObject)
				return new ObjectExplorer(jsonObject, new());
		}

		//: treat results as array
		{
			var jsonArray = res.Matches.Select(x => x.Value).ToJsonArray();
			return new ArrayExplorer(jsonArray, new());
		}
	}
}
