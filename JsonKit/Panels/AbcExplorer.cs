using FarNet;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

abstract class AbcExplorer : Explorer
{
	public AbcExplorer(Guid typeId) : base(typeId)
	{
		CanGetContent = true;
		CanOpenFile = true;
	}

	protected static readonly JsonSerializerOptions _jsonSerializerOptions1 = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	};

	protected readonly JsonSerializerOptions _jsonSerializerOptions2 = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		WriteIndented = true,
	};

	public override void EnterPanel(Panel panel)
	{
		panel.Title = ToString()!;
	}

	public sealed override Explorer? OpenFile(OpenFileEventArgs args)
	{
		var node = args.File.Data;

		if (node is JsonArray jsonArray)
			return new ArrayExplorer(jsonArray);

		if (node is JsonObject jsonObject)
			return new ObjectExplorer(jsonObject);

		return null;
	}

	public sealed override void GetContent(GetContentEventArgs args)
	{
		var node = args.File.Data as JsonNode;
		if (node is null)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.UseText = node is JsonValue ? node.ToString() : node.ToJsonString(_jsonSerializerOptions2);
		args.UseFileExtension = node is JsonValue ? "txt" : "json";
	}
}
