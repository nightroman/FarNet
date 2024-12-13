using FarNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

class ArrayExplorer(JsonArray target, ExplorerArgs args)
	: AbcExplorer(MyTypeId, args)
{
	public static Guid MyTypeId = new("f457df2e-85f9-430a-9ac3-3e3c69d3e027");
	readonly JsonArray _target = target;
	FarFile[]? _files;

	public override JsonNode JsonNode => _target;

	protected override void UpdateFile(NodeFile file, JsonNode? node)
	{
		// ensure the file is live
		if (_files?.Contains(file) != true)
			throw Errors.CannotFindSource();

		// set by the last node index
		var last = file.Node;
		if (last is null)
		{
			// ensure the last index node is still null
			int index = file.Index;
			if (index >= _target.Count || _target[index] is { })
				throw Errors.CannotFindSource();

			_target[index] = node;
		}
		else if (node?.Parent is { } parent)
		{
			// noop if new node parent is this target
			if (parent != _target)
				throw Errors.CannotFindSource();
		}
		else
		{
			// set new node by the index of old
			int index = _target.IndexOf(last);
			if (index < 0)
				throw Errors.CannotFindSource();

			_target[index] = node;
		}

		// reset in place
		file.SetNode(node);

		UpdateParent(_target);
	}

	public override string ToString()
	{
		return $"Array {_target.GetPath()}";
	}

	public override Panel CreatePanel()
	{
		return new ArrayPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		if (_files is null)
		{
			_files = new FarFile[_target.Count];
			int index = _files.Length;
			while (--index >= 0)
				_files[index] = new NodeFile(_target[index], index);
		}
		return _files;
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
		if (args.Force)
		{
			base.DeleteFiles(args);
			return;
		}

		foreach (var file in args.Files.OrderByDescending(x => x.Length))
			_target.RemoveAt((int)file.Length);

		_files = null;
		UpdateParent(_target);
	}
}
