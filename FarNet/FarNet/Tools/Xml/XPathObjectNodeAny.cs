using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FarNet.Tools;

/*
	Type attributes contain actual value type names in non-null cases.
	Nulls are represented as empty text elements where Type is:
	- Property item: property type name
	- List item: "Object"
*/

class XPathObjectNodeAny : XPathObjectNode
{
	readonly object _tag;
	readonly string _name;

	readonly Type? _type;
	readonly string? _key;

	public XPathObjectNodeAny(XPathObjectContext? context, object tag)
		: this(context ?? new XPathObjectContext(), tag, null, null, null, null, -1)
	{
	}

	XPathObjectNodeAny(
		XPathObjectContext context,
		object tag,
		Type? type,
		string? key,
		XPathObjectNodeAny? parent,
		IList<XPathObjectNode>? siblings,
		int index)
		: base(
			context,
			parent,
			siblings,
			index)
	{
		_tag = tag ?? throw new ArgumentNullException(nameof(tag));
		_type = type;
		_key = key;

		if (tag is IEnumerable && tag is not string)
			_name = context.NameTable.Add("List");
		else
			_name = context.NameTable.Add("Item");
	}

	public override object Tag => _tag;

	public override string Name => _name;

	public override bool HasText =>
		IsLinearType(_tag);

	public override string? Value =>
		LinearTypeToString(_tag);

	protected override void ActivateAttributes()
	{
		if (_attributes is not null)
			return;

		var attrType = new ValueGetter("Type", _ => _type is null ? _tag.GetType().Name : _type.Name);

		if (string.IsNullOrEmpty(_key))
		{
			_attributes = [attrType];
		}
		else
		{
			_attributes = [new ValueGetter("Name", it => _key), attrType];
		}
	}

	protected override IList<XPathObjectNode> ActivateElements()
	{
		if (_elements.Target is IList<XPathObjectNode> alive)
			return alive;

		if (_context.CancellationToken.IsCancellationRequested || _context.Depth >= 0 && _depth >= _context.Depth)
			return _emptyElements;

		if (IsLinearType(_tag))
		{
			_elements.Target = _emptyElements;
			return _emptyElements;
		}

		List<XPathObjectNode> elements = _tag switch
		{
			IDictionary dictionary => ActivateDictionary(dictionary),
			IEnumerable collection => ActivateCollection(collection),
			_ => ActivateItem(),
		};

		if (elements.Count == 0)
		{
			_elements.Target = _emptyElements;
			return _emptyElements;
		}

		_elements.Target = elements;
		return elements;
	}

	List<XPathObjectNode> ActivateCollection(IEnumerable collection)
	{
		var elements = new List<XPathObjectNode>(collection is ICollection list ? list.Count : 0);
		foreach (object? value in collection)
		{
			elements.Add(new XPathObjectNodeAny(
				_context, value ?? string.Empty,
				value is null ? typeof(object) : null,
				null,
				this,
				elements,
				elements.Count));
		}

		return elements;
	}

	List<XPathObjectNode> ActivateDictionary(IDictionary dictionary)
	{
		var elements = new List<XPathObjectNode>(dictionary.Count);
		foreach (DictionaryEntry kv in dictionary)
		{
			var value = kv.Value;
			elements.Add(new XPathObjectNodeAny(
				_context,
				value ?? string.Empty,
				value is null ? typeof(object) : null,
				kv.Key?.ToString() ?? string.Empty,
				this,
				elements,
				elements.Count));
		}

		return elements;
	}

	List<XPathObjectNode> ActivateItem()
	{
		var elements = new List<XPathObjectNode>();

		// public fields, e.g. of value tuples
		foreach (FieldInfo info in _tag.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
		{
			object? value = info.GetValue(_tag);

			elements.Add(new XPathObjectNodeAny(
				_context,
				value ?? string.Empty,
				value is null ? info.FieldType : null,
				info.Name,
				this,
				elements,
				elements.Count));
		}

		// public properties
		foreach (PropertyInfo info in _tag.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			object? value;
			try
			{
				value = info.GetValue(_tag, null);
			}
			catch
			{
				value = null;
			}

			elements.Add(new XPathObjectNodeAny(
				_context,
				value ?? string.Empty,
				value is null ? info.PropertyType : null,
				info.Name,
				this,
				elements,
				elements.Count));
		}

		return elements;
	}
}
