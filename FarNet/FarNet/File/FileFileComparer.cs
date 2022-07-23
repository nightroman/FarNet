
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Compares files by their references.
/// </summary>
public sealed class FileFileComparer : EqualityComparer<FarFile>
{
	/// <inheritdoc/>
	public override bool Equals(FarFile x, FarFile y)
	{
		return object.Equals(x, y);
	}

	/// <inheritdoc/>
	public override int GetHashCode(FarFile obj)
	{
		return obj == null ? 0 : obj.GetHashCode();
	}
}
