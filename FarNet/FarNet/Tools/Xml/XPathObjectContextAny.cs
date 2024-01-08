
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Tools;

/// <summary>
/// XPath context.
/// </summary>
public class XPathObjectContextAny : XPathObjectContext
{
	internal string DataElementName { get; }
	internal string ItemElementName { get; }
	internal string ListElementName { get; }

	XPathObjectNodeAny? _rootNode;

	/// <summary>
	/// .
	/// </summary>
	public XPathObjectContextAny()
	{
		DataElementName = NameTable.Add("data");
		ItemElementName = NameTable.Add("item");
		ListElementName = NameTable.Add("list");
	}

	/// <summary>
	/// The root object.
	/// </summary>
	public object Root
	{
		get => _rootNode!.Tag;
		set => _rootNode = new XPathObjectNodeAny(this, value);
	}

	internal override XPathObjectNode RootNode => _rootNode!;
}
