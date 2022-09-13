
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;

namespace PowerShellFar;

/// <summary>
/// Abstract table explorer.
/// </summary>
public abstract class TableExplorer : PowerExplorer
{
	/// <inheritdoc/>
	protected TableExplorer(Guid typeId) : base(typeId)
	{
	}

	/// <include file='doc.xml' path='doc/Columns/*'/>
	internal virtual object[]? Columns { get; set; }
}
