
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

// http://msdn.microsoft.com/en-us/library/ms950764.aspx

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace FarNet.Tools;

class XPathObjectNode
{
	static readonly XmlAttributeInfo[] _emptyAttributes = [];
	static readonly XPathObjectNode[] _emptyElements = [];
	readonly XPathObjectContext _context;
	readonly object _target;
	readonly string _name;
	readonly XPathObjectNode? _parent;
	readonly int _depth;

	// Sibling list, elements of the parent (it keeps the weak reference alive).
	readonly IList<XPathObjectNode>? _siblings;

	// Index of this node in the sibling list, needed for MoveToNext, MoveToPrevious.
	readonly int _index;

	IList<XmlAttributeInfo>? _attributes;
	readonly WeakReference _elements = new(null);

	public XPathObjectNode(XPathObjectContext context, object target) : this(context, target, null, null, null, -1)
	{
	}

	XPathObjectNode(XPathObjectContext context, object target, string? name, XPathObjectNode? parent, IList<XPathObjectNode>? siblings, int index)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
		_target = target ?? throw new ArgumentNullException(nameof(target));
		_parent = parent;
		_siblings = siblings;
		_index = index;

		if (parent is not null)
			_depth = parent._depth + 1;

		if (string.IsNullOrEmpty(name))
		{
			if (target is IXmlInfo info)
			{
				name = info.XmlNodeName();
			}
			else
			{
				var type = target.GetType();
				name = target.GetType().Name;
				if (type.IsGenericType)
					name = name.Remove(name.IndexOf('`'));
			}
		}
		_name = GetAtomicString(name);
	}

	public object Target => _target;

	public int Index => _index;

	public string Name => _name;

	public XPathObjectNode? Parent => _parent;

	public string? Value
	{
		get
		{
			if (HasText)
				return CultureSafeToString(_target);

			return string.Empty;
		}
	}

	public bool HasAttributes
	{
		get
		{
			if (_attributes is null)
				ActivateAttributes();

			return _attributes!.Count > 0;
		}
	}

	public bool HasChildren
	{
		get
		{
			var elements = (_elements.Target as IList<XPathObjectNode>) ?? ActivateElements();
			return elements.Count > 0 || HasText;
		}
	}

	public bool HasText
	{
		get
		{
			Type type = _target.GetType();

			return type.IsValueType || type == typeof(string);
		}
	}

	public IList<XmlAttributeInfo> Attributes
	{
		get
		{
			if (_attributes is null)
				ActivateAttributes();

			return _attributes!;
		}
	}

	public string GetAttributeValue(string name)
	{
		if (_attributes is null)
			ActivateAttributes();

		foreach (var it in _attributes!)
		{
			if (it.Name == name)
				return CultureSafeToString(it.Getter(_target)) ?? string.Empty;
		}

		return string.Empty;
	}

	public IList<XPathObjectNode> Elements
	{
		get
		{
			return (_elements.Target as IList<XPathObjectNode>) ?? ActivateElements();
		}
	}

	public void AddSpecialName(string key, string? value)
	{
		if (_attributes is null)
			ActivateAttributes();

		// clone if read only
		if (_attributes!.IsReadOnly)
			_attributes = new List<XmlAttributeInfo>(_attributes);

		_attributes.Add(new XmlAttributeInfo("*" + key, (object v) => value));
	}

	void ActivateAttributes()
	{
		if (_context.Stopping is not null && _context.Stopping(EventArgs.Empty))
		{
			//! ensure at least dummy, a caller expects not null
			_attributes = _emptyAttributes;
			return;
		}

		if (_attributes is not null)
			return;

		if (_target is SuperFile)
		{
			ActivateSuperFileAttributes();
		}
		else if (_target is ValueType || _target is string) //? "Linear types". Perhaps we need more.
		{
			// no attributes or children
			_attributes = _emptyAttributes;
			_elements.Target = _emptyElements;
		}
		else if (_target is IDictionary) //! before ICollection
		{
			ActivateDictionary();
		}
		else if (_target is ICollection) //! after IDictionary
		{
			ActivateCollection();
		}
		else
		{
			ActivateSimple();
		}
	}

	IList<XPathObjectNode> ActivateElements()
	{
		if (_context.Stopping is not null && _context.Stopping(EventArgs.Empty))
			return _emptyElements;

		{
			if (_elements.Target is IList<XPathObjectNode> elements)
				return elements;
		}

		if (_target is SuperFile)
		{
			return ActivateSuperFileElements();
		}
		else if (_target is ValueType || _target is string) //? "Linear types". Perhaps we need more.
		{
			// no attributes or children
			_attributes = _emptyAttributes;
			_elements.Target = _emptyElements;
			return _emptyElements;
		}
		else if (_target is IDictionary) //! before ICollection
		{
			return ActivateDictionary();
		}
		else if (_target is ICollection) //! after IDictionary
		{
			return ActivateCollection();
		}
		else
		{
			return ActivateSimple();
		}
	}

	List<XPathObjectNode> ActivateDictionary()
	{
		// no attributes
		_attributes = _emptyAttributes;

		// collect elements
		var elements = new List<XPathObjectNode>();

		foreach (DictionaryEntry entry in (IDictionary)_target)
		{
			if (entry.Value is null)
				continue;

			var node = new XPathObjectNode(_context, entry.Value, null, this, elements, elements.Count);

			elements.Add(node);

			node.AddSpecialName("key", entry.Key.ToString());
		}

		if (elements.Count == 0)
			_elements.Target = _emptyElements;
		else
			_elements.Target = elements;

		return elements;
	}

	List<XPathObjectNode> ActivateCollection()
	{
		// no attributes
		_attributes = _emptyAttributes;

		// collect elements
		var elements = new List<XPathObjectNode>();
		foreach (object it in (ICollection)_target)
		{
			if (it is not null)
				elements.Add(new XPathObjectNode(_context, it, null, this, elements, elements.Count));
		}

		if (elements.Count == 0)
			_elements.Target = _emptyElements;
		else
			_elements.Target = elements;

		return elements;
	}

	IList<XPathObjectNode> ActivateSimple()
	{
		//?? need?
		if (_target is IXmlInfo info)
		{
			_attributes = info.XmlAttributes();
			if (_attributes.Count == 0)
				_attributes = _emptyAttributes;

			_elements.Target = _emptyElements; //? elements
			return _emptyElements;
		}

		_attributes = new List<XmlAttributeInfo>();
		var elements = new List<XPathObjectNode>();

		foreach (PropertyInfo pi in _target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			// get the value
			object? value;
			try
			{
				value = pi.GetValue(_target, null);
				if (value is null)
					continue;
			}
			catch
			{
				continue;
			}

			// get the custom attributes
			//? It is done just to skip XmlIgnoreAttribute. Do we need this expensive job?
			object[] attrs = pi.GetCustomAttributes(true);
			bool skip = false;

			if (attrs is not null)
			{
				foreach (var a in attrs.Cast<Attribute>())
				{
					if (a is System.Xml.Serialization.XmlIgnoreAttribute)
					{
						skip = true;
						break;
					}
				}
			}

			if (skip)
				continue; //? It was break == bug?

			// now handle the values
			var str = CultureSafeToString(value);

			if (str is not null)
				_attributes.Add(new XmlAttributeInfo(GetAtomicString(pi.Name), (object v) => str));
			else
				elements.Add(new XPathObjectNode(_context, value, pi.Name, this, elements, elements.Count));
		}

		if (elements.Count == 0)
			_elements.Target = _emptyElements;
		else
			_elements.Target = elements;

		return elements;
	}

	void ActivateSuperFileAttributes()
	{
		var file = (SuperFile)_target;
		_attributes = file.XmlAttributes();
	}

	IList<XPathObjectNode> ActivateSuperFileElements()
	{
		var file = (SuperFile)_target;

		if (!file.IsDirectory)
		{
			_elements.Target = _emptyElements;
			return _emptyElements;
		}

		// progress
		_context.IncrementDirectoryCount?.Invoke(1);

		// explore and get files
		List<XPathObjectNode>? elements = null;
		if (_context.Depth < 0 || _depth < _context.Depth)
		{
			Explorer? explorer2 = SuperExplorer.ExploreSuperDirectory(file.Explorer, ExplorerModes.Find, file);
			if (explorer2 is not null)
			{
				var args = new GetFilesEventArgs(ExplorerModes.Find);
				var files2 = explorer2.GetFiles(args);
				var count = files2 is ICollection collection ? collection.Count : 0;

				elements = new(count);
				foreach (var file2 in files2)
				{
					// filter files
					if (!file2.IsDirectory && (_context.SkipFiles || _context.Filter is not null && !_context.Filter(explorer2, file2)))
						continue;

					// add
					elements.Add(new XPathObjectNode(
						_context,
						new SuperFile(explorer2, file2),
						null,
						this,
						elements,
						elements.Count));
				}
			}
		}

		if (elements is null || elements.Count == 0)
		{
			_elements.Target = _emptyElements;
			return _emptyElements;
		}
		else
		{
			_elements.Target = elements;
			return elements;
		}
	}

	string GetAtomicString(string array)
	{
		return _context.NameTable.Get(array) ?? _context.NameTable.Add(array);
	}

	internal static string? CultureSafeToString(object? value)
	{
		// string
		if (value is string asString)
			return asString;

		if (value is ValueType)
		{
			// DateTime
			if (value is DateTime asDateTime)
			{
				return asDateTime.ToString(asDateTime.TimeOfDay.Ticks > 0 ? "yyyy-MM-dd HH:mm:ss" : "yyyy-MM-dd", null);
			}

			// Boolean
			if (value is bool asBool)
			{
				return asBool ? "1" : "0";
			}

			// specific handling for floating point types
			if ((value is decimal) || (value is float) || (value is double))
				return ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture.NumberFormat);

			// generic handling for all other value types
			return value.ToString();
		}

		// objects return null
		return null;
	}
}
