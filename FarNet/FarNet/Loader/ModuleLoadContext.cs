
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Reflection;
using System.Runtime.Loader;

namespace FarNet.Works;

class ModuleLoadContext : AssemblyLoadContext
{
	readonly AssemblyDependencyResolver _resolver;

	public ModuleLoadContext(string pluginPath)
	{
		_resolver = new AssemblyDependencyResolver(pluginPath);
	}

	protected override Assembly Load(AssemblyName assemblyName)
	{
		string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
		if (assemblyPath != null)
		{
			Log.Source.TraceInformation("Load managed {0}", assemblyPath);
			return LoadFromAssemblyPath(assemblyPath);
		}

		return null;
	}

	protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
	{
		string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
		if (libraryPath != null)
		{
			Log.Source.TraceInformation("Load native {0}", libraryPath);
			return LoadUnmanagedDllFromPath(libraryPath);
		}

		return IntPtr.Zero;
	}
}
