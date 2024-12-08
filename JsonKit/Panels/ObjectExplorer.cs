﻿using FarNet;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

class ObjectExplorer(JsonObject target, Parent? parent, string? filePath = null)
	: AbcExplorer(MyTypeId, parent, filePath)
{
	public static Guid MyTypeId = new("2dfece07-d75b-41cc-bf81-c6fcccf8b63e");
	readonly JsonObject _target = target;
	List<FarFile>? _files;

	public override JsonNode JsonNode => _target;

	protected override void ResetFile(SetFile file, JsonNode? node)
	{
		file.Description = node is null ? "null" : node.ToJsonString(OptionsPanel);
		file.Data = node;

		if (node?.Parent is null)
			_target[file.Name] = node;

		ParentResetFile(_target);
	}

	public override string ToString()
	{
		return "Object";
	}

	public override Panel CreatePanel()
	{
		return new ObjectPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		if (_files is null)
		{
			_files = [];
			int index = -1;
			CollectionsMarshal.SetCount(_files, _target.Count);
			var span = CollectionsMarshal.AsSpan(_files);
			foreach (var kv in _target)
			{
				++index;
				span[index] = new SetFile
				{
					Name = kv.Key,
					Description = kv.Value is null ? "null" : kv.Value.ToJsonString(OptionsPanel),
					Data = kv.Value,
				};
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
		ParentResetFile(_target);
	}
}
