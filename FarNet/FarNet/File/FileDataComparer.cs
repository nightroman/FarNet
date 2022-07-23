
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Compares files by their <see cref="FarFile.Data"/> references.
/// </summary>
public sealed class FileDataComparer : EqualityComparer<FarFile>
{
	/// <inheritdoc/>
	public override bool Equals(FarFile x, FarFile y)
	{
		if (x == null || y == null)
			return x == null && y == null;
		else
			return object.Equals(x.Data, y.Data);
	}

	/// <inheritdoc/>
	public override int GetHashCode(FarFile obj)
	{
		return (obj == null || obj.Data == null) ? 0 : obj.Data.GetHashCode();
	}
}
