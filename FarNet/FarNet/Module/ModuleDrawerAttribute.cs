
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Module editor drawer attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleDrawerAttribute : ModuleActionAttribute
{
	/// <include file='doc.xml' path='doc/FileMask/*'/>
	public string Mask { get; set; }
	/// <summary>
	/// Color priority.
	/// </summary>
	public int Priority { get; set; }
}
