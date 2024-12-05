using FarNet;
using System;
using System.Collections.Generic;
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
		foreach (var (propertyName, node) in _target)
		{
			var file = new SetFile
			{
				Name = propertyName,
				Description = node is null ? "null" : node.ToJsonString(_jsonSerializerOptions1),
				Data = node,
			};
			_files.Add(file);
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
