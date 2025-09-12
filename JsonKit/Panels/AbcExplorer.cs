﻿using FarNet;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

abstract class AbcExplorer : Explorer
{
	static readonly HashSet<JsonNode> _dirty = [];

	readonly ExplorerArgs _args;
	protected NodeFile[]? _files;

	public AbcExplorer(Guid typeId, ExplorerArgs args) : base(typeId)
	{
		_args = args;

		CanDeleteFiles = true;
		CanGetContent = true;
		CanSetText = true;
		CanOpenFile = true;
	}

	public abstract JsonNode Node { get; }
	public ExplorerArgs Args => _args;

	public bool IsDirty()
	{
		return _dirty.Contains(Node.Root);
	}

	internal void SetDirty()
	{
		_dirty.Add(Node.Root);
		ResetParentFile();
	}

	void ResetParentFile()
	{
		if (_args.Parent is { } parent)
		{
			parent.File.Reset();
			parent.Explorer.ResetParentFile();
		}
	}

	public void SaveData()
	{
		if (_args.FilePath is { } filePath)
		{
			var text = Node.Root.ToJsonString(JsonOptions.Editor);
			File.WriteAllText(filePath, text);
		}

		_dirty.Remove(Node.Root);
	}

	protected abstract void UpdateNode(NodeFile file, JsonNode? node);

	internal void UpdateFile(NodeFile file, JsonNode? node)
	{
		// ensure the file is live (vs lost on deleting nodes)
		if (_files?.Contains(file) != true)
			throw Errors.CannotFindSource();

		// update the node
		UpdateNode(file, node);

		// reset in place
		file.SetNode(node);
	}

	internal void PanelClosed()
	{
		if (_args.Parent is null)
			_dirty.Remove(Node.Root);
	}

	public sealed override Explorer? OpenFile(OpenFileEventArgs args)
	{
		var file = (NodeFile)args.File;
		var node = file.Node;

		if (node is JsonArray jsonArray)
			return new ArrayExplorer(jsonArray, _args with { Parent = new(this, file) });

		if (node is JsonObject jsonObject)
			return new ObjectExplorer(jsonObject, _args with { Parent = new(this, file) });

		return null;
	}

	public sealed override void GetContent(GetContentEventArgs args)
	{
		var file = (NodeFile)args.File;
		var node = file.Node;

		args.UseText = node is JsonValue ? node.ToString() : node is { } ? node.ToJsonString(JsonOptions.Editor) : "null";
		args.UseFileExtension = node is JsonValue ? "txt" : "json";
		args.CanSet = true;
	}

	public sealed override void SetText(SetTextEventArgs args)
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

		SetDirty();
		UpdateFile(file, node2);
	}

	protected abstract void DeleteFiles2(DeleteFilesEventArgs args);

	public sealed override void DeleteFiles(DeleteFilesEventArgs args)
	{
		SetDirty();

		if (args.Force)
		{
			foreach (var file in args.Files.Cast<NodeFile>())
				UpdateFile(file, null);
		}
		else
		{
			DeleteFiles2(args);
			_files = null;
		}
	}

	internal NodeFile? FindFileByNodeParents(JsonNode node)
	{
		if (_files is null)
			throw new InvalidOperationException("Unexpected null files.");

		for (var parent = node; parent is { }; parent = parent.Parent)
		{
			if (_files.FirstOrDefault(x => x.Node == parent) is { } file)
				return file;
		}

		return null;
	}
}
