
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

//! DictionaryEntry is not good for this, because it is a value type.
//! DataItem is a reference type with some advantages.
/// <summary>
/// Named data item, e.g. an info panel item (<see cref="Panel.InfoItems"/>).
/// </summary>
/// <param name="name">Name (or separator text in some cases).</param>
/// <param name="data">Data (or null for separator in some cases).</param>
public class DataItem(string name, object data)
{

	/// <summary>
	/// Name (or separator text in some cases).
	/// </summary>
	public string Name { get; set; } = name;

	/// <summary>
	/// Data (or null for separator in some cases).
	/// </summary>
	public object Data { get; set; } = data;
}
