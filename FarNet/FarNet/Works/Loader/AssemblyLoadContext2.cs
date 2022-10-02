
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Reflection;
using System.Runtime.Loader;

namespace FarNet.Works;

class AssemblyLoadContext2 : AssemblyLoadContext
{
	readonly AssemblyDependencyResolver _resolver;

	public AssemblyLoadContext2(string pluginPath, bool isCollectible = false) : base(isCollectible)
	{
		_resolver = new AssemblyDependencyResolver(pluginPath);
	}

	protected override Assembly? Load(AssemblyName assemblyName)
	{
		var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
		if (assemblyPath is null)
			return null;

		Log.Source.TraceInformation("Load managed {0}", assemblyPath);
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
