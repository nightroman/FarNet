
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace FarNet.Works;

class AssemblyLoadContext2(string pluginPath, bool isCollectible = false) : AssemblyLoadContext(isCollectible)
{
	readonly AssemblyDependencyResolver _resolver = new(pluginPath);

	protected override Assembly? Load(AssemblyName assemblyName)
	{
		var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
		if (assemblyPath is null)
			return null;

		Debug.WriteLine($"## ALC {assemblyPath}");

		return LoadFromAssemblyPath(assemblyPath);
	}

	protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
	{
		var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
		if (libraryPath is null)
			return IntPtr.Zero;

		//! do not trace, not so useful, too many (same), e.g. JavaScriptFar
		return LoadUnmanagedDllFromPath(libraryPath);
	}
}
