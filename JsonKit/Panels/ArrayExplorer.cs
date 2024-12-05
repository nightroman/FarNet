using FarNet;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

class ArrayExplorer : AbcExplorer
{
	public static Guid MyTypeId = new("f457df2e-85f9-430a-9ac3-3e3c69d3e027");
	readonly JsonArray _target;
	readonly bool _isMultipleValues;
	readonly List<FarFile> _files;

	public ArrayExplorer(JsonArray target, bool isMultipleValues = false) : base(MyTypeId)
	{
		_target = target;
		_isMultipleValues = isMultipleValues;

		_files = [];
		var index = -1;
		foreach (var node in _target)
		{
			++index;
			var file = new SetFile
			{
				Name = node is null ? "null" : node.ToJsonString(_jsonSerializerOptions1),
				Length = index,
				Data = node,
			};
			_files.Add(file);
		}
	}

	public override string ToString()
	{
		return _isMultipleValues ? "Values" : "Array";
	}

	public override Panel CreatePanel()
	{
		return new ArrayPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		return _files;
	}
}
