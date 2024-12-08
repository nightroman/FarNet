using FarNet;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

class ObjectExplorer : AbcExplorer
{
	public static Guid MyTypeId = new("2dfece07-d75b-41cc-bf81-c6fcccf8b63e");
	readonly JsonObject _target;
	readonly List<FarFile> _files;

	public ObjectExplorer(JsonObject target) : base(MyTypeId)
	{
		_target = target;

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
				Description = kv.Value is null ? "null" : kv.Value.ToJsonString(_jsonSerializerOptions1),
				Data = kv.Value,
			};
		}
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
		return _files;
	}
}
