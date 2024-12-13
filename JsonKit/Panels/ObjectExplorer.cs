using FarNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

class ObjectExplorer(JsonObject target, ExplorerArgs args)
	: AbcExplorer(MyTypeId, args)
{
	public static Guid MyTypeId = new("2dfece07-d75b-41cc-bf81-c6fcccf8b63e");
	readonly JsonObject _target = target;
	FarFile[]? _files;

	public override JsonNode JsonNode => _target;

	protected override void UpdateFile(NodeFile file, JsonNode? node)
	{
		// ensure the file is live
		if (_files?.Contains(file) != true)
			throw Errors.CannotFindSource();

		// ensure the property exists
		if (!_target.ContainsKey(file.Name))
			throw Errors.CannotFindSource();

		// set by property name
		_target[file.Name] = node;

		// reset in place
		file.SetNode(node);

		UpdateParent(_target);
	}

	public override string ToString()
	{
		return $"Object {_target.GetPath()}";
	}

	public override Panel CreatePanel()
	{
		return new ObjectPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		if (_files is null)
		{
			_files = new FarFile[_target.Count];
			int index = -1;
			foreach (var kv in _target)
			{
				++index;
				_files[index] = new NodeFile(kv.Value, kv.Key);
			}
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

		foreach (var file in args.Files)
			_target.Remove(file.Name);

		_files = null;
		UpdateParent(_target);
	}
}
