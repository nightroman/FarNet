
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

// http://msdn.microsoft.com/en-us/library/ms950764.aspx

using System;
using System.Collections.Generic;
using System.Globalization;

namespace FarNet.Tools;

abstract class XPathObjectNode
{
	protected static readonly XPathObjectNode[] _emptyElements = [];
	protected readonly XPathObjectContext _context;
	protected readonly int _depth;

	readonly XPathObjectNode? _parent;

	// Sibling list, elements of the parent (it keeps the weak reference alive).
	readonly IList<XPathObjectNode>? _siblings;

	// Index of this node in the sibling list, needed for MoveToNext, MoveToPrevious.
	readonly int _index;

	protected IList<ValueGetter>? _attributes;
	protected readonly WeakReference _elements = new(null);

	public abstract object Tag { get; }
	public abstract string Name { get; }
	protected abstract void ActivateAttributes();
	protected abstract IList<XPathObjectNode> ActivateElements();

	protected XPathObjectNode(XPathObjectContext context, XPathObjectNode? parent, IList<XPathObjectNode>? siblings, int index)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
		_parent = parent;
		_siblings = siblings;
		_index = index;

		if (parent is not null)
			_depth = parent._depth + 1;
	}

	public XPathObjectContext Context =>
		_context;

	public XPathObjectNode? Parent =>
		_parent;

	public int Index =>
		_index;

	public virtual string? Value =>
		null;

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

	public virtual bool HasText =>
		false;

	public IList<ValueGetter> Attributes
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
				return LinearTypeToString(it.Value(Tag)) ?? string.Empty;
		}

		return string.Empty;
	}

	public IList<XPathObjectNode> Elements =>
		(_elements.Target as IList<XPathObjectNode>) ?? ActivateElements();

	internal static bool IsLinearType(object value)
	{
		return
			value is string ||
			value is IFormattable ||
			value is bool ||
			value is CultureInfo;
	}

	internal static string? LinearTypeToString(object? value)
	{
		// string
		if (value is string asString)
			return asString;

		// all formattable
		if (value is IFormattable formattable)
		{
			// DateTime
			if (value is DateTime asDateTime)
				return asDateTime.ToString(asDateTime.TimeOfDay.Ticks > 0 ? "yyyy-MM-dd HH:mm:ss" : "yyyy-MM-dd", null);

			return formattable.ToString(null, CultureInfo.InvariantCulture);
		}

		// Boolean, 1/0 seems to be easier to use in XPath
		if (value is bool asBool)
			return asBool ? "1" : "0";

		// special types
		if (value is CultureInfo)
			return value.ToString();

		// objects return null
		return null;
	}
}
