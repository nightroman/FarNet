
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

// http://msdn.microsoft.com/en-us/library/ms950764.aspx

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;

#pragma warning disable 1591
namespace FarNet.Tools;

public class XPathObjectNavigator : XPathNavigator
{
	const string UnexpectedNode = "Unexpected node type.";

	XPathObjectContext _context;
	XPathObjectNode _node;
	XPathNodeType _type;
	int _index = -1;

	public XPathObjectNavigator(object root, int depth) : this(new XPathObjectContextAny { Root = root, Depth = depth })
	{
	}

	public XPathObjectNavigator(XPathObjectContext context)
	{
		_context = context;
		_node = context.RootNode;
	}

	XPathObjectNavigator(XPathObjectNavigator that)
	{
		_context = that._context;
		_node = that._node;
		_type = that._type;
		_index = that._index;
	}

	// Uses our functions.
	public override XPathExpression Compile(string xpath)
	{
		return Compile(xpath, null);
	}

	// Uses our functions and variables.
	public XPathExpression Compile(string xpath, IDictionary? variables)
	{
		var xsltContext = new XPathXsltContext(_context.NameTable);
		if (variables is { })
		{
			foreach (DictionaryEntry kv in variables)
				xsltContext.AddVariable(kv.Key.ToString()!, kv.Value!);
		}

		var expression = base.Compile(xpath);
		expression.SetContext(xsltContext);

		return expression;
	}

	// Use before HasElements and Elements in MoveToFirstChild (places that activate elements).
	// Why? To return valid text and attributes and complete navigation of activated elements.
	bool IsCancellationRequested =>
		_context.Depth >= 0 && _node.Depth >= _context.Depth || _context.CancellationToken.IsCancellationRequested;

	void MoveNavigator(XPathObjectNode that)
	{
		_node = that;
		_type = XPathNodeType.Element;
		_index = -1;
	}

	public override object UnderlyingObject =>
		_node.Tag;

	public override string BaseURI =>
		string.Empty;

	public override bool HasAttributes =>
		_type == XPathNodeType.Element && _node.HasAttributes;

	public override bool HasChildren => _type switch
	{
		XPathNodeType.Element => _node.HasText || !IsCancellationRequested && _node.HasElements,
		XPathNodeType.Root => true,
		_ => false,
	};

	public override bool IsEmptyElement =>
		!HasChildren;

	public override string LocalName =>
		Name;

	public override string Name => _type switch
	{
		XPathNodeType.Element => _node.Name,
		XPathNodeType.Attribute => _node.Attributes[_index].Name,
		_ => throw new InvalidOperationException(UnexpectedNode),
	};

	public override string NamespaceURI =>
		string.Empty;

	public override XPathNodeType NodeType =>
		_type;

	public override XmlNameTable NameTable =>
		_context.NameTable;

	public override string Prefix =>
		string.Empty;

	public override string Value => _type switch
	{
		XPathNodeType.Attribute => XPathObjectNode.DataToString(_node.Attributes[_index].Value(UnderlyingObject)),
		XPathNodeType.Element => _node.Value ?? string.Empty,
		XPathNodeType.Text => _node.Value ?? string.Empty,
		_ => throw new InvalidOperationException(UnexpectedNode)
	};

	public override string XmlLang =>
		string.Empty;

	public override XPathNavigator Clone() =>
		new XPathObjectNavigator(this);

	public override string GetAttribute(string localName, string namespaceURI)
	{
		if (_type == XPathNodeType.Element)
		{
			if (string.IsNullOrEmpty(namespaceURI))
				return _node.GetAttributeValue(localName);
		}

		return string.Empty;
	}

	public override string GetNamespace(string name) =>
		string.Empty;

	public override bool IsDescendant(XPathNavigator? nav)
	{
		if (nav is not XPathObjectNavigator that)
			return false;

		// different trees? - no
		if (_context != that._context)
			return false;

		// inside my tree? - yes
		if (_type == XPathNodeType.Root && that._type != XPathNodeType.Root)
			return true;

		// my node is not on element - no, attributes and text have no descendants
		if (_type != XPathNodeType.Element)
			return false;

		// same node but it is not on an element - yes, it is my attribute or text
		if (_node == that._node)
			return that._type != XPathNodeType.Element;

		// find my node in its parents
		for (var parent = that._node.Parent; parent is { }; parent = parent.Parent)
		{
			if (parent == _node)
				return true;
		}

		return false;
	}

	public override bool IsSamePosition(XPathNavigator other)
	{
		if (other is not XPathObjectNavigator that)
			return false;

		// different node? - no
		if (_node != that._node)
			return false;

		// different types? - no
		if (_type != that._type)
			return false;

		// on different attributes? - no
		if (_type == XPathNodeType.Attribute && _index != that._index)
			return false;

		return true;
	}

	public override bool MoveTo(XPathNavigator other)
	{
		if (other is not XPathObjectNavigator that)
			return false;

		_context = that._context;
		_node = that._node;
		_type = that._type;
		_index = that._index;

		return true;
	}

	public override bool MoveToAttribute(string localName, string namespaceURI)
	{
		if (_type != XPathNodeType.Element)
			return false;

		var attributes = _node.Attributes;
		for (int index = attributes.Count; --index >= 0;)
		{
			if (attributes[index].Name == localName)
			{
				_type = XPathNodeType.Attribute;
				_index = index;
				return true;
			}
		}

		return false;
	}

	// NB Not used in our XPath scans
	public override bool MoveToFirst()
	{
		// must be false on attributes
		if (_type == XPathNodeType.Attribute)
			return false;

		// move to the parent
		if (!MoveToParent())
			return false;

		// and its first child
		return MoveToFirstChild();
	}

	public override bool MoveToFirstAttribute()
	{
		if (!HasAttributes)
			return false;

		_type = XPathNodeType.Attribute;
		_index = 0;
		return true;
	}

	public override bool MoveToFirstChild()
	{
		// move to the root node
		if (_type == XPathNodeType.Root)
		{
			MoveNavigator(_context.RootNode);
			return true;
		}

		// only element may have children
		if (_type != XPathNodeType.Element)
			return false;

		// move to the text
		if (_node.HasText)
		{
			_type = XPathNodeType.Text;
			_index = -1;
			return true;
		}

		// cancel?
		if (IsCancellationRequested)
			return false;

		// move to the first element
		var elements = _node.Elements;
		if (elements.Count > 0)
		{
			MoveNavigator(elements[0]);
			return true;
		}

		return false;
	}

	public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope) =>
		false;

	public override bool MoveToId(string id) =>
		false;

	public override bool MoveToNamespace(string name) =>
		false;

	public override bool MoveToNext()
	{
		if (_type != XPathNodeType.Element)
			return false;

		var parent = _node.Parent;
		if (parent is null)
			return false;

		var elements = parent.Elements;
		int index = _node.Index + 1;
		if (index >= elements.Count)
			return false;

		MoveNavigator(elements[index]);
		return true;
	}

	public override bool MoveToNextAttribute()
	{
		if (_type != XPathNodeType.Attribute)
			return false;

		if (_index + 1 >= _node.Attributes.Count)
			return false;

		++_index;
		return true;
	}

	public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope) =>
		false;

	public override bool MoveToParent()
	{
		if (_type == XPathNodeType.Root)
			return false;

		if (_type != XPathNodeType.Element)
		{
			_type = XPathNodeType.Element;
			return true;
		}

		var parent = _node.Parent;
		if (parent is null)
			return false;

		MoveNavigator(parent);
		return true;
	}

	public override bool MoveToPrevious()
	{
		if (_type != XPathNodeType.Element)
			return false;

		var parent = _node.Parent;
		if (parent is null)
			return false;

		var elements = parent.Elements;
		int index = _node.Index - 1;
		if (index < 0)
			return false;

		MoveNavigator(elements[index]);
		return true;
	}

	public override void MoveToRoot()
	{
		_type = XPathNodeType.Root;
		_node = _context.RootNode;
		_index = -1;
	}
}
