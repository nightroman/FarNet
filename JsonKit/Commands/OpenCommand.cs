using FarNet;
using JsonKit.About;
using JsonKit.Panels;
using System;
using System.Data.Common;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonKit.Commands;

sealed class OpenCommand(DbConnectionStringBuilder parameters) : AnyCommand
{
	readonly string _path = parameters.GetRequiredString(Host.Param.File);

	public override void Invoke()
	{
		var fullPath = Environment.ExpandEnvironmentVariables(_path);
		if (!Path.IsPathRooted(fullPath))
			fullPath = Path.Join(Far.Api.CurrentDirectory, fullPath);

		var bytes = File.ReadAllBytes(fullPath);
		var index = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
		var span = bytes.AsSpan(index);

		JsonReaderOptions options = new() { AllowMultipleValues = true, AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
		Utf8JsonReader reader = new(span, options);

		var nodes = JsonNodeReader.ReadNodes(ref reader);

		if (nodes.Count == 1 && nodes[0] is JsonArray jsonArray)
		{
			ArrayExplorer explorer = new(jsonArray);
			explorer.OpenPanel();
			return;
		}

		if (nodes.Count == 1 && nodes[0] is JsonObject jsonObject)
		{
			ObjectExplorer explorer = new(jsonObject);
			explorer.OpenPanel();
			return;
		}

		{
			jsonArray = new JsonArray([.. nodes]);
			ArrayExplorer explorer = new(jsonArray, true);
			explorer.OpenPanel();
		}
	}
}
