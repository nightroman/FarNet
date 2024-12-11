using FarNet;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

abstract class AbcExplorer : Explorer
{
	readonly string? _filePath;
	readonly Parent? _parent;
	bool _isDirty;

	public AbcExplorer(Guid typeId, Parent? parent, string? filePath = null) : base(typeId)
	{
		_parent = parent;
		_filePath = filePath;

		CanDeleteFiles = true;
		CanGetContent = true;
		CanSetText = true;
		CanOpenFile = true;
	}

	public abstract JsonNode JsonNode { get; }

	public Parent? Parent => _parent;

	public bool IsDirty()
	{
		return _isDirty || (_parent is { } && _parent.Explorer.IsDirty());
	}

	public void SetIsDirty(bool isDirty)
	{
		_isDirty = isDirty;
		if (_parent is { })
			_parent.Explorer.SetIsDirty(isDirty);
	}

	public void SaveData()
	{
		if (_filePath is { })
		{
			var text = JsonNode.ToJsonString(OptionsEditor);
			File.WriteAllText(_filePath, text);
		}
		else
		{
			_parent!.Explorer.SaveData();
		}

		_isDirty = false;
	}

	protected abstract void UpdateFile(NodeFile file, JsonNode? node);

	protected void UpdateParent(JsonNode? node)
	{
		SetIsDirty(true);
		_parent?.Explorer.UpdateFile(_parent.File, node);
	}

	internal static readonly JsonSerializerOptions OptionsEditor = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		WriteIndented = true,
	};

	internal static readonly JsonSerializerOptions OptionsPanel = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	};

	public override void EnterPanel(Panel panel)
	{
		panel.Title = ToString()!;
	}

	public sealed override Explorer? OpenFile(OpenFileEventArgs args)
	{
		var file = (NodeFile)args.File;
		var node = file.Node;

		if (node is JsonArray jsonArray)
			return new ArrayExplorer(jsonArray, new(this, file));

		if (node is JsonObject jsonObject)
			return new ObjectExplorer(jsonObject, new(this, file));

		return null;
	}

	public sealed override void GetContent(GetContentEventArgs args)
	{
		var file = (NodeFile)args.File;
		var node = file.Node;

		args.UseText = node is JsonValue ? node.ToString() : node is { } ? node.ToJsonString(OptionsEditor) : "null";
		args.UseFileExtension = node is JsonValue ? "txt" : "json";
		args.CanSet = true;
	}

	public override void SetText(SetTextEventArgs args)
	{
		var file = (NodeFile)args.File;
		var node1 = file.Node;

		JsonNode? node2;
		if (node1 is null || node1.GetValueKind() != JsonValueKind.String)
		{
			node2 = JsonNode.Parse(args.Text, documentOptions: new()
			{
				AllowTrailingCommas = true,
				CommentHandling = JsonCommentHandling.Skip,
			});
		}
		else
		{
			node2 = JsonValue.Create(args.Text);
		}

		UpdateFile(file, node2);
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
		foreach(var file in args.Files)
			UpdateFile((NodeFile)file, null);
	}
}
