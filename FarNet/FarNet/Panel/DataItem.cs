
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

//! DictionaryEntry is not good for this, because it is a value type.
//! DataItem is a reference type with some advantages.
/// <summary>
/// Named data item, e.g. an info panel item (<see cref="Panel.InfoItems"/>).
/// </summary>
public class DataItem
{
	/// <summary>
	/// New completely defined data item.
	/// </summary>
	/// <param name="name">Name (or separator text in some cases).</param>
	/// <param name="data">Data (or null for separator in some cases).</param>
	public DataItem(string name, object data)
	{
		Name = name;
		Data = data;
	}

	/// <summary>
	/// Name (or separator text in some cases).
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Data (or null for separator in some cases).
	/// </summary>
	public object Data { get; set; }
}
