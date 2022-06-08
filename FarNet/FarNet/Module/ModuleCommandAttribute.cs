
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Module command attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleCommandAttribute : ModuleActionAttribute
{
	/// <summary>
	/// The mandatory not empty command prefix.
	/// </summary>
	/// <remarks>
	/// This prefix is only a suggestion, the actual prefix is configured by a user.
	/// </remarks>
	public string Prefix { get; set; }
}
