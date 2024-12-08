using FarNet;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
		CollectionsMarshal.SetCount(_files, _target.Count);
		var span = CollectionsMarshal.AsSpan(_files);
		for (int index = 0; index < span.Length; ++index)
		{
			var node = _target[index];
			span[index] = new SetFile
			{
				Name = node is null ? "null" : node.ToJsonString(_jsonSerializerOptions1),
				Length = index,
				Data = node,
			};
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
