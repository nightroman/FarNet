using FarNet;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

sealed class ArrayExplorer(JsonArray node, ExplorerArgs args)
	: AbcExplorer(MyTypeId, args)
{
	public static Guid MyTypeId = new("f457df2e-85f9-430a-9ac3-3e3c69d3e027");
	readonly JsonArray _node = node;

	public override JsonNode Node => _node;

	protected override void UpdateNode(NodeFile file, JsonNode? node)
	{
		// set by the node index
		_node[file.Index] = node;
	}

	public override Panel CreatePanel()
	{
		return new ArrayPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		if (_files is null)
		{
			_files = new NodeFile[_node.Count];
			int index = _files.Length;
			while (--index >= 0)
				_files[index] = new NodeFile(_node[index], index);
		}
		return _files;
	}

	protected override void DeleteFiles2(DeleteFilesEventArgs args)
	{
		foreach (var file in args.Files.OrderByDescending(x => x.Length))
			_node.RemoveAt((int)file.Length);
	}

	internal bool MoveItem(NodeFile file, int delta)
	{
		int index1 = file.Index;
		int index2 = index1 + delta;
		if (index2 < 0 || index2 >= _node.Count)
			return false;

		SetDirty();
		_files = null;
		_node.RemoveAt(index1);
		_node.Insert(index2, file.Node);

		return true;
	}
}
