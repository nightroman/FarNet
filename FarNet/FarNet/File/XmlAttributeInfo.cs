
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>.</summary>
/// <remarks>.</remarks>
/// <param name="name">.</param>
/// <param name="getter">.</param>
public class XmlAttributeInfo(string name, Func<object, object?> getter)
{
	/// <summary>.</summary>
	public string Name { get; } = name;

	/// <summary>.</summary>
	public Func<object, object?> Getter { get; } = getter;
}
