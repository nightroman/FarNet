
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Compares files by their names.
/// </summary>
public sealed class FileNameComparer : EqualityComparer<FarFile>
{
	readonly StringComparer _comparer;

	/// <summary>
	/// New comparer with the <c>OrdinalIgnoreCase</c> string comparer.
	/// </summary>
	public FileNameComparer()
	{
		_comparer = StringComparer.OrdinalIgnoreCase;
	}

	/// <summary>
	/// New comparer with the specified string comparer.
	/// </summary>
	/// <param name="comparer">The string comparer.</param>
	public FileNameComparer(StringComparer comparer)
	{
		_comparer = comparer ?? throw new ArgumentNullException("comparer");
	}

	/// <inheritdoc/>
	public override bool Equals(FarFile x, FarFile y)
	{
		if (x == null || y == null)
			return x == null && y == null;
		else
			return _comparer.Equals(x.Name, y.Name);
	}

	/// <inheritdoc/>
	public override int GetHashCode(FarFile obj)
	{
		return obj == null || obj.Name == null ? 0 : obj.Name.GetHashCode();
	}
}
