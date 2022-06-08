
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Module tool action attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleToolAttribute : ModuleActionAttribute
{
	/// <summary>
	/// The tool options with at least one target area specified.
	/// </summary>
	public ModuleToolOptions Options { get; set; }
}
