﻿using FarNet;
using JsonKit.About;
using JsonKit.Panels;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonKit.Commands;

sealed class OpenCommand(CommandParameters parameters) : AnyCommand
{
	readonly string _path = parameters.GetRequiredString(Host.Param.File, expandVariables: true, resolveFullPath: true);

	public override void Invoke()
	{
		var bytes = File.ReadAllBytes(_path);
		var index = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
		var span = bytes.AsSpan(index);

		JsonReaderOptions options = new() { AllowMultipleValues = true, AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
		Utf8JsonReader reader = new(span, options);

		var nodes = JsonNodeReader.ReadNodes(ref reader);

		if (nodes.Count == 1 && nodes[0] is JsonArray jsonArray)
		{
			ArrayExplorer explorer = new(jsonArray, null, filePath: _path);
			explorer.OpenPanel();
			return;
		}

		if (nodes.Count == 1 && nodes[0] is JsonObject jsonObject)
		{
			ObjectExplorer explorer = new(jsonObject, null, filePath: _path);
			explorer.OpenPanel();
			return;
		}

		{
			jsonArray = new JsonArray([.. nodes]);
			ArrayExplorer explorer = new(jsonArray, null, filePath: _path);
			explorer.OpenPanel();
		}
	}
}
