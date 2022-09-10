
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Tools;

/// <summary>
/// Abstract history store.
/// </summary>
public abstract class HistoryStore
{
	/// <summary>
	/// Gets true if the store can add new lines.
	/// </summary>
	public virtual bool CanAdd { get; }

	/// <summary>
	/// Gets history lines.
	/// </summary>
	public abstract string[] ReadLines();

	/// <summary>
	/// Adds a new history line.
	/// </summary>
	/// <param name="line">The text line to add.</param>
	public virtual void AddLine(string line)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Updates the input lines and returns updated.
	/// </summary>
	/// <param name="lines">The text lines to update.</param>
	public virtual string[] Update(string[]? lines)
	{
		throw new NotImplementedException();
	}
}
