
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>.</summary>
public class XmlAttributeInfo
{
	/// <summary>.</summary>
	/// <param name="name">.</param>
	/// <param name="getter">.</param>
	public XmlAttributeInfo(string name, Func<object, object?> getter)
	{
		Name = name;
		Getter = getter;
	}

	/// <summary>.</summary>
	public string Name { get; }

	/// <summary>.</summary>
	public Func<object, object?> Getter { get; }
}
