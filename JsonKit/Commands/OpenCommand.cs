using FarNet;
using Json.More;
using Json.Path;
using JsonKit.About;
using JsonKit.Panels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonKit.Commands;

sealed class OpenCommand : AbcCommand
{
	// from file
	readonly string? _file;
	readonly JsonPath? _select;

	// from panel
	readonly (AbcPanel, JsonPath) _panel;

	public OpenCommand(CommandParameters parameters)
	{
		_file = parameters.GetPath(Param.File, ParameterOptions.UseCursorFile);

		if (parameters.GetString(Param.Select) is { } select)
		{
			try { _select = JsonPath.Parse(select); }
			catch (Exception ex) { throw parameters.ParameterError(Param.Select, ex.Message); }
		}

		if (_file is null)
		{
			if (_select is null || Far.Api.Panel is not AbcPanel panel)
				throw parameters.ParameterError(Param.File, "Omitted requires the panel cursor file.");

			_panel = (panel, _select);
		}
	}

	public override void Invoke()
	{
		//: select from json panel
		if (_file is null)
		{
			var (panel, select) = _panel;
			var exp = panel.MyExplorer;
			CreateAbcExplorer(exp.Node, select, exp.Args.FilePath, exp).OpenPanelChild(panel);
			return;
		}

		byte[] bytes;
		try { bytes = File.ReadAllBytes(_file); }
		catch (Exception ex) { throw new ModuleException(ex.Message); }

		var index = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
		var span = bytes.AsSpan(index);

		JsonReaderOptions options = new() { AllowMultipleValues = true, AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
		Utf8JsonReader reader = new(span, options);

		List<JsonNode?> nodes;
		try { nodes = JsonNodeReader.ReadNodes(ref reader); }
		catch (Exception ex) { throw new ModuleException(ex.Message); }

		AbcExplorer explorer;
		if (nodes.Count == 1 && nodes[0] is JsonArray jsonArray)
		{
			if (_select is null)
			{
				explorer = new ArrayExplorer(jsonArray, new() { FilePath = _file });
			}
			else
			{
				explorer = CreateAbcExplorer(jsonArray, _select, _file);
			}
		}
		else if (nodes.Count == 1 && nodes[0] is JsonObject jsonObject)
		{
			if (_select is null)
			{
				explorer = new ObjectExplorer(jsonObject, new() { FilePath = _file });
			}
			else
			{
				explorer = CreateAbcExplorer(jsonObject, _select, _file);
			}
		}
		else
		{
			jsonArray = new JsonArray([.. nodes]);
			if (_select is null)
			{
				explorer = new ArrayExplorer(jsonArray, new());
			}
			else
			{
				explorer = CreateAbcExplorer(jsonArray, _select);
			}
		}

		explorer.OpenPanel();
	}

	static AbcExplorer CreateAbcExplorer(
		JsonNode root,
		JsonPath select,
		string? filePath = null,
		AbcExplorer? explorer = null)
	{
		//! select and convert, avoid 2+ enumerations, "Deferred Execution"
		var nodes = select.Evaluate(root).Matches.ToList();

		//: singular array or object
		if (select.IsSingular && nodes.Count == 1)
		{
			var node = nodes[0].Value;

			Parent? parent = null;
			if (node is { } && explorer is { })
			{
				var file = explorer.FindFileByNodeParents(node) ??
					throw new InvalidOperationException("Cannot find file by node parents.");

				parent = new(explorer, file);
			}

			if (node is JsonArray jsonArray)
				return new ArrayExplorer(jsonArray, new() { FilePath = filePath, Parent = parent });

			if (node is JsonObject jsonObject)
				return new ObjectExplorer(jsonObject, new() { FilePath = filePath, Parent = parent });
		}

		//: clone as array
		{
			var jsonArray = nodes.Select(x => x.Value).ToJsonArray();
			return new ArrayExplorer(jsonArray, new());
		}
	}
}
