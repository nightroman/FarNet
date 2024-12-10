using FarNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

class ArrayExplorer(JsonArray target, Parent? parent, string? filePath = null)
	: AbcExplorer(MyTypeId, parent, filePath)
{
	public static Guid MyTypeId = new("f457df2e-85f9-430a-9ac3-3e3c69d3e027");
	readonly JsonArray _target = target;
	List<FarFile>? _files;

	public override JsonNode JsonNode => _target;

	protected override void UpdateFile(SetFile file, JsonNode? node)
	{
		// ensure the file is live
		if (_files?.Contains(file) != true)
			throw Errors.CannotFindSource();

		// set by the last node index
		var last = (JsonNode?)file.Data;
		if (last is null)
		{
			// ensure the last index node is still null
			int index = (int)file.Length;
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

		// update the file in place
		file.Name = node is null ? "null" : node.ToJsonString(OptionsPanel);
		file.Data = node;

		UpdateParent(_target);
	}

	public override string ToString()
	{
		return "Array";
	}

	public override Panel CreatePanel()
	{
		return new ArrayPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		if (_files is null)
		{
			_files = [];
			CollectionsMarshal.SetCount(_files, _target.Count);
			var span = CollectionsMarshal.AsSpan(_files);
			for (int index = 0; index < span.Length; ++index)
			{
				var node = _target[index];
				span[index] = new SetFile
				{
					Name = node is null ? "null" : node.ToJsonString(OptionsPanel),
					Length = index,
					Data = node,
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

		foreach (var file in args.Files.OrderByDescending(x => x.Length))
			_target.RemoveAt((int)file.Length);

		_files = null;
		UpdateParent(_target);
	}
}
