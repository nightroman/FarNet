
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Concurrent;

namespace FarNet;

/// <summary>
/// Utilities for modules and scripts.
/// </summary>
public static class User
{
	/// <summary>
	/// Gets the concurrent dictionary suitable for cross thread and module operations.
	/// </summary>
	static public ConcurrentDictionary<string, object> Data { get; } = [];
}
