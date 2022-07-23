
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// INTERNAL
/// </summary>
public class XmlAttributeInfo
{
	/// <summary>
	/// INTERNAL
	/// </summary>
	/// <param name="name">INTERNAL</param>
	/// <param name="getter">INTERNAL</param>
	public XmlAttributeInfo(string name, Func<object, object> getter)
	{
		Name = name;
		Getter = getter;
	}

	/// <summary>
	/// INTERNAL
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// INTERNAL
	/// </summary>
	public Func<object, object> Getter { get; }
}
