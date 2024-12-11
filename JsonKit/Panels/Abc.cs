using FarNet;
using System.Text.Json.Nodes;

namespace JsonKit.Panels;

static class Errors
{
	public static ModuleException CannotFindSource() => new("Cannot find the source.");
}

sealed class Parent(AbcExplorer explorer, NodeFile file)
{
	public AbcExplorer Explorer => explorer;
	public NodeFile File => file;
}

// Represents an array item or object property.
sealed class NodeFile : FarFile
{
	// array item: null, object property: name
	readonly string? _name;

	// array item: index, object property: 0
	readonly int _index;

	JsonNode? _node;
	string? _json;

	// Array item.
	public NodeFile(JsonNode? node, int index)
	{
		_node = node;
		_index = index;
	}

	// Object property.
	public NodeFile(JsonNode? node, string name)
	{
		_node = node;
		_name = name;
	}

	// Current node.
	public JsonNode? Node => _node;

	// Array item index.
	public int Index => _index;

	// Sets a new node.
	public void SetNode(JsonNode? node)
	{
		_node = node;
		_json = null;
	}

	string GetJson() =>
		_json ??= _node is null ? "null" : _node.ToJsonString(AbcExplorer.OptionsPanel);

	public override string Name =>
		_name is null ? GetJson() : _name;

	public override string? Description =>
		_name is null ? null : GetJson();

	public override long Length =>
		_index;
}
