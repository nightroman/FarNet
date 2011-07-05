
// http://msdn.microsoft.com/en-us/library/ms950764.aspx

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace FarNet.Tools
{
	///
	public class XPathObjectNavigator : XPathNavigator
	{
		XPathObjectContext _context;
		XPathObjectNode _root;
		XPathObjectNode _node;
		XPathNodeType _type;
		int _index = -1;
		///
		internal XPathObjectNavigator(object root, XPathObjectContext context)
		{
			if (root == null) throw new ArgumentNullException("root");
			if (context == null) throw new ArgumentNullException("context");

			_context = context;
			_root = new XPathObjectNode(context, root);

			//???? fails without it
			var type = root.GetType();
			var name = type.FullName;
			if (type.IsGenericType)
				name = name.Remove(name.IndexOf('`'));
			_root.AddSpecialName("type", name);
		}
		///
		public XPathObjectNavigator(object root) : this(root, new XPathObjectContext()) { }
		XPathObjectNavigator(XPathObjectNavigator that)
		{
			_context = that._context;
			_root = that._root;
			_node = that._node;
			_type = that._type;
			_index = that._index;
		}
		void MoveNavigator(XPathObjectNode that)
		{
			_node = that;
			_type = XPathNodeType.Element;
			_index = -1;
		}
		///
		public override object UnderlyingObject { get { return _node.Target; } }
		///
		public override string BaseURI
		{
			// don't expose a namespace
			get { return string.Empty; }
		}
		///
		public override bool HasAttributes
		{
			get
			{
				// nothing has attributes except elements
				return _type == XPathNodeType.Element && _node.HasAttributes;
			}
		}
		///
		public override bool HasChildren
		{
			get
			{
				switch (_type)
				{
					case XPathNodeType.Element:
						// does the element have children?
						return _node.HasChildren;

					case XPathNodeType.Root:
						// the root always has at least one child
						// (the object the navigator is built from)
						return true;

					default:
						// nothing else has children
						return false;
				}
			}
		}
		///
		public override bool IsEmptyElement
		{
			get
			{
				// empty if we don't have children
				return !HasChildren;
			}
		}
		///
		public override string LocalName
		{
			// we don't use namespaces, so our Name and LocalName are the same
			get { return Name; }
		}
		///
		public override string Name
		{
			get
			{
				switch (_type)
				{
					case XPathNodeType.Element:
						return _node.Name;

					case XPathNodeType.Attribute:
						var attrs = _node.Attributes;
						if (_index >= 0 && _index < attrs.Count)
						{
							var data = attrs[_index].Name;

							if (data[0] == '*')
								data = data.Substring(1);

							return data;
						}
						break;
				}
				return string.Empty;
			}
		}
		///
		public override string NamespaceURI
		{
			get
			{
				switch (_type)
				{
					case XPathNodeType.Attribute:
						var attrs = _node.Attributes;
						if (_index >= 0 && _index < attrs.Count)
						{
							string data = attrs[_index].Name;

							if (data[0] == '*')
								return "urn:ObjectXPathNavigator";
						}
						break;
				}
				return string.Empty;
			}
		}
		///
		public override XPathNodeType NodeType
		{
			get { return _type; }
		}
		///
		public override XmlNameTable NameTable
		{
			get { return _context.NameTable; }
		}
		///
		public override string Prefix
		{
			get
			{
				switch (_type)
				{
					case XPathNodeType.Attribute:
						var attrs = _node.Attributes;
						if (_index >= 0 && _index < attrs.Count)
						{
							string data = attrs[_index].Name;

							if (data[0] == '*')
								return "oxp";
						}
						break;
				}
				return string.Empty;
			}
		}
		///
		public override string Value
		{
			get
			{
				switch (_type)
				{
					case XPathNodeType.Attribute:
						var attrs = _node.Attributes;
						if (_index >= 0 && _index < attrs.Count)
						{
							var info = attrs[_index];
							return XPathObjectNode.CultureSafeToString(info.Getter(UnderlyingObject)) ?? string.Empty;
						}
						break;

					case XPathNodeType.Element:
						return _node.Value;

					case XPathNodeType.Text:
						goto case XPathNodeType.Element;
				}

				return string.Empty;
			}
		}
		///
		public override string XmlLang
		{
			get { return string.Empty; }
		}
		///
		public override XPathNavigator Clone()
		{
			return new XPathObjectNavigator(this);
		}
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
		public override string GetNamespace(string name)
		{
			return string.Empty;
		}
		///
		public override bool IsDescendant(XPathNavigator nav)
		{
			var that = nav as XPathObjectNavigator;
			if (that == null)
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
			for (var parent = that._node.Parent; parent != null; parent = parent.Parent)
			{
				if (parent == _node)
					return true;
			}

			return false;
		}
		///
		public override bool IsSamePosition(XPathNavigator other)
		{
			var that = other as XPathObjectNavigator;
			if (that == null)
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
			var that = other as XPathObjectNavigator;
			if (that == null)
				return false;

			_context = that._context;
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
		public override bool MoveToFirst() //???? see MSDN. When is it called? Is it ever called on XPath scan?
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
			if (_type != XPathNodeType.Element)
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
		public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
		{
			return false;
		}
		///
		public override bool MoveToId(string id)
		{
			return false;
		}
		///
		public override bool MoveToNamespace(string name)
		{
			return false;
		}
		///
		public override bool MoveToNext()
		{
			if (_type != XPathNodeType.Element)
				return false;

			XPathObjectNode parent = _node.Parent;
			if (parent == null)
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
		public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
		{
			return false;
		}
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

			XPathObjectNode parent = _node.Parent;
			if (parent == null)
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

			XPathObjectNode parent = _node.Parent;

			if (parent == null)
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
			_node = null;
			_index = -1;
		}
	}
}
