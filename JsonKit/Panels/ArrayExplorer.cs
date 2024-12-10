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

	protected override void ResetFile(SetFile file, JsonNode? node)
	{
		file.Name = node is null ? "null" : node.ToJsonString(OptionsPanel);
		file.Data = node;

		if (node?.Parent is null)
			_target[(int)file.Length] = node;

		ParentResetFile(_target);
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
		ParentResetFile(_target);
	}
}
