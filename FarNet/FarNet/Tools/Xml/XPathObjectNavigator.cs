
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

// http://msdn.microsoft.com/en-us/library/ms950764.aspx

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;

namespace FarNet.Tools;

///
public class XPathObjectNavigator : XPathNavigator
{
	const string UnexpectedNode = "Unexpected node type.";

	XPathObjectNode _root;
	XPathObjectNode _node;
	XPathNodeType _type;
	int _index = -1;

	///
	public XPathObjectNavigator(object root, int depth) : this(new XPathObjectNodeAny(new XPathObjectContext { Depth = depth }, root))
	{
	}

	internal XPathObjectNavigator(SuperFile root, XPathObjectContext context) : this(new XPathObjectNodeFile(context, root))
	{
	}

	XPathObjectNavigator(XPathObjectNode root)
	{
		_root = root;
		_node = root;
	}

	XPathObjectNavigator(XPathObjectNavigator that)
	{
		_root = that._root;
		_node = that._node;
		_type = that._type;
		_index = that._index;
	}

	///
	// Uses our functions.
	public override XPathExpression Compile(string xpath)
	{
		return Compile(xpath, null);
	}

	///
	// Uses our functions and variables.
	public XPathExpression Compile(string xpath, IDictionary? variables)
	{
		var xsltContext = new XPathXsltContext(_root.Context.NameTable);
		if (variables is { })
		{
			foreach (DictionaryEntry kv in variables)
				xsltContext.AddVariable(kv.Key.ToString()!, kv.Value!);
		}

		var expression = base.Compile(xpath);
		expression.SetContext(xsltContext);

		return expression;
	}

	void MoveNavigator(XPathObjectNode that)
	{
		_node = that;
		_type = XPathNodeType.Element;
		_index = -1;
	}

	///
	public override object UnderlyingObject =>
		_node.Tag;

	///
	public override string BaseURI =>
		string.Empty;

	///
	public override bool HasAttributes =>
		_type == XPathNodeType.Element && _node.HasAttributes;

	///
	public override bool HasChildren => _type switch
	{
		XPathNodeType.Element => _node.HasChildren,
		XPathNodeType.Root => true,
		_ => false,
	};

	///
	public override bool IsEmptyElement =>
		!HasChildren;

	///
	public override string LocalName =>
		Name;

	///
	public override string Name => _type switch
	{
		XPathNodeType.Element => _node.Name,
		XPathNodeType.Attribute => _node.Attributes[_index].Name,
		_ => throw new InvalidOperationException(UnexpectedNode),
	};

	///
	public override string NamespaceURI =>
		string.Empty;

	///
	public override XPathNodeType NodeType =>
		_type;

	///
	public override XmlNameTable NameTable =>
		_root.Context.NameTable;

	///
	public override string Prefix =>
		string.Empty;

	///
	public override string Value => _type switch
	{
		XPathNodeType.Attribute => XPathObjectNode.LinearTypeToString(_node.Attributes[_index].Value(UnderlyingObject)) ?? string.Empty,
		XPathNodeType.Element => _node.Value ?? string.Empty,
		XPathNodeType.Text => _node.Value ?? string.Empty,
		_ => throw new InvalidOperationException(UnexpectedNode)
	};

	///
	public override string XmlLang =>
		string.Empty;

	///
	public override XPathNavigator Clone() =>
		new XPathObjectNavigator(this);

	///
	public override string GetAttribute(string localName, string namespaceURI)
	{
		if (_type == XPathNodeType.Element)
		{
			if (string.IsNullOrEmpty(namespaceURI))
				return _node.GetAttributeValue(localName);
		}

		return string.Empty;
	}

	///
	public override string GetNamespace(string name) =>
		string.Empty;

	///
	public override bool IsDescendant(XPathNavigator? nav)
	{
		if (nav is not XPathObjectNavigator that)
			return false;

		// if they're in different graphs, they're not the same
		if (_root != that._root)
			return false;

		// if its on my root element - its still a descendant
		if (_type == XPathNodeType.Root && that._type != XPathNodeType.Root)
			return true;

		// if I'm not on an element, it can't be my descendant
		// (attributes and text don't have descendants)
		if (_type != XPathNodeType.Element)
			return false;

		// if its on my attribute or content - its still a descendant
		if (_node == that._node)
			return (_type == XPathNodeType.Element && that._type != XPathNodeType.Element);

		// ok, we need to hunt...
		for (var parent = that._node.Parent; parent is not null; parent = parent.Parent)
		{
			if (parent == _node)
				return true;
		}

		return false;
	}

	///
	public override bool IsSamePosition(XPathNavigator other)
	{
		if (other is not XPathObjectNavigator that)
			return false;

		// if they're in different graphs, they're not the same
		if (_root != that._root)
			return false;

		// if they're different node types, they can't be the same node!
		if (_type != that._type)
			return false;

		// if they're different elements, they can't be the same node!
		if (_node != that._node)
			return false;

		// are they on different attributes?
		if (_type == XPathNodeType.Attribute && _index != that._index)
			return false;

		return true;
	}

	///
	public override bool MoveTo(XPathNavigator other)
	{
		if (other is not XPathObjectNavigator that)
			return false;

		_root = that._root;
		_node = that._node;
		_type = that._type;
		_index = that._index;

		return true;
	}

	///
	public override bool MoveToAttribute(string localName, string namespaceURI)
	{
		if (_type != XPathNodeType.Element)
			return false;

		int index = -1;
		foreach (var it in _node.Attributes)
		{
			++index;
			if (it.Name == localName)
			{
				_type = XPathNodeType.Attribute;
				_index = index;
				return true;
			}
		}

		return false;
	}

	///
	public override bool MoveToFirst() //? When is it called? Is it ever called on XPath scan?
	{
		// The original code was wrong. We use the code similar to sdf.XPath.
		if (_type == XPathNodeType.Attribute)
			return false;

		// try the parent
		if (!MoveToParent())
			return false;

		// and then its child
		return MoveToFirstChild();
	}

	///
	public override bool MoveToFirstAttribute()
	{
		if (!HasAttributes)
			return false;

		_type = XPathNodeType.Attribute;
		_index = 0;

		return true;
	}

	///
	public override bool MoveToFirstChild()
	{
		if (_type == XPathNodeType.Root)
		{
			// move to the document element
			MoveNavigator(_root);
			return true;
		}

		if (_type != XPathNodeType.Element)
			return false;

		// drop down to the text value (if any)
		if (_node.HasText)
		{
			_type = XPathNodeType.Text;
			_index = -1;

			return true;
		}

		// drop down to the first element (if any)
		var elems = _node.Elements;
		if (elems.Count > 0)
		{
			MoveNavigator(elems[0]);
			return true;
		}

		return false;
	}

	///
	public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope) =>
		false;

	///
	public override bool MoveToId(string id) =>
		false;

	///
	public override bool MoveToNamespace(string name) =>
		false;

	///
	public override bool MoveToNext()
	{
		if (_type != XPathNodeType.Element)
			return false;

		var parent = _node.Parent;
		if (parent is null)
			return false;

		var elems = parent.Elements;
		int next = _node.Index + 1;
		if (next >= elems.Count)
			return false;

		MoveNavigator(elems[next]);
		return true;
	}

	///
	public override bool MoveToNextAttribute()
	{
		if (_type != XPathNodeType.Attribute)
			return false;

		if (_index + 1 >= _node.Attributes.Count)
			return false;

		++_index;
		return true;
	}

	///
	public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope) =>
		false;

	///
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

	///
	public override bool MoveToPrevious()
	{
		if (_type != XPathNodeType.Element)
			return false;

		if (_type != XPathNodeType.Element)
			return false;

		var parent = _node.Parent;

		if (parent is null)
			return false;

		var elems = parent.Elements;
		int prev = _node.Index - 1;
		if (prev < 0)
			return false;

		MoveNavigator(elems[prev]);
		return true;
	}

	///
	public override void MoveToRoot()
	{
		_type = XPathNodeType.Root;
		_node = _root;
		_index = -1;
	}
}
