
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

// http://msdn.microsoft.com/en-us/library/ms950764.aspx

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace FarNet.Tools;

abstract class XPathObjectNode
{
	protected static ValueGetter[] EmptyAttributes => [];
	protected static XPathObjectNode[] EmptyElements => [];

	public XPathObjectNode? Parent { get; }
	public int Depth { get; }

	// Index of this node in the sibling list for MoveToNext, MoveToPrevious.
	public int Index { get; }

	// Sibling list, elements of the parent, to keeps the weak reference alive.
	readonly IList<XPathObjectNode>? _siblings;

	IList<ValueGetter>? _attributes;
	readonly WeakReference _elements = new(null);

	protected XPathObjectNode(XPathObjectNode? parent, IList<XPathObjectNode>? siblings, int index)
	{
		Parent = parent;
		if (parent is not null)
			Depth = parent.Depth + 1;

		_siblings = siblings;
		Index = index;
	}

	public abstract object Tag { get; }

	public abstract string Name { get; }

	public virtual bool HasText =>
		false;

	public virtual string? Value =>
		null;

	protected virtual IList<ValueGetter> GetAttributes()
		=> EmptyAttributes;

	protected virtual IList<XPathObjectNode> GetElements()
		=> EmptyElements;

	public bool HasAttributes =>
		Attributes.Count > 0;

	public bool HasElements =>
		Elements.Count > 0;

	// Gets or uses ready attributes.
	public IList<ValueGetter> Attributes =>
		_attributes ??= GetAttributes();

	// Gets or uses alive weak elements.
	public IList<XPathObjectNode> Elements
	{
		get
		{
			if (_elements.Target is IList<XPathObjectNode> alive)
				return alive;

			alive = GetElements();
			_elements.Target = alive;

			return alive;
		}
	}

	public string GetAttributeValue(string name)
	{
		foreach (var it in Attributes)
		{
			if (it.Name == name)
				return LinearTypeToString(it.Value(Tag));
		}

		return string.Empty;
	}

	public static bool IsLinearType(object value)
	{
		return
			value is string ||
			value is IFormattable ||
			value is bool ||
			value is CultureInfo;
	}

	/// <summary>
	/// Called for attributes and objects tested as linear.
	/// </summary>
	public static string LinearTypeToString(object? value)
	{
		if (value is null)
			return string.Empty;

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

		// Boolean, 1/0 is easier to use in XPath
		if (value is bool asBool)
			return asBool ? "1" : "0";

		// other types, e.g. CultureInfo
		return value.ToString()!;
	}
}
