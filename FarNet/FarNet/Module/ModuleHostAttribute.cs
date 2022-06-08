
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Module host attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleHostAttribute : Attribute
{
	/// <summary>
	/// Tells to load and connect the host always. There should be good reasons for 'true'.
	/// </summary>
	/// <remarks>
	/// If the module host is the only implemented module item then this flag
	/// has to be set to true. Otherwise the host has no chances to be used.
	/// </remarks>
	public bool Load { get; set; }
}
