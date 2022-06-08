
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Module editor action attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleEditorAttribute : ModuleActionAttribute
{
	/// <include file='doc.xml' path='doc/FileMask/*'/>
	public string Mask { get; set; }
}
