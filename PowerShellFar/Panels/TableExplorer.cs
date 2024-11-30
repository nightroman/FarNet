
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;

namespace PowerShellFar;

/// <summary>
/// Abstract table explorer.
/// </summary>
/// <inheritdoc/>
public abstract class TableExplorer(Guid typeId) : PowerExplorer(typeId)
{
	/// <include file='doc.xml' path='doc/Columns/*'/>
	internal virtual object[]? Columns { get; set; }
}
