using FarNet;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

sealed class ObjectExplorer : AbcExplorer
{
	public static Guid MyTypeId = new("2dfece07-d75b-41cc-bf81-c6fcccf8b63e");
	readonly JsonObject _node;

	public override JsonNode Node => _node;

	public ObjectExplorer(JsonObject node, ExplorerArgs args) : base(MyTypeId, args)
	{
		_node = node;
	}

	protected override void UpdateNode(NodeFile file, JsonNode? node)
	{
		// set by property name
		_node[file.Name] = node;
	}

	public override Panel CreatePanel()
	{
		return new ObjectPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		if (_files is null)
		{
			_files = new NodeFile[_node.Count];
			int index = -1;
			foreach (var kv in _node)
			{
				++index;
				_files[index] = new NodeFile(kv.Value, kv.Key);
			}
		}
		return _files;
	}

	protected override void DeleteFiles2(DeleteFilesEventArgs args)
	{
		foreach (var file in args.Files)
			_node.Remove(file.Name);
	}
}
