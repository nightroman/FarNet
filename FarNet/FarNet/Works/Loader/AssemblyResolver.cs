
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FarNet.Works;
#pragma warning disable 1591

// Tempted to append to assembly cache dynamically on each module loading from
// its directory and skip ALC modules at all. This is not good, e.g. modules
// may refer to FSharp.Core from FSF; they fail if FSF is not in the cache,
// skipped for any reason (ALC or not yet loaded).
// -- So for now keep loading all (Modules, Lib) to the cache.

public static class AssemblyResolver
{
	const int MaxLastRoots = 4;
	static readonly LinkedList<string> s_lastRoots = new();
	static readonly Dictionary<string, object?> s_cache = new(StringComparer.OrdinalIgnoreCase);

	static AssemblyResolver()
	{
		var root = Path.GetDirectoryName(typeof(AssemblyResolver).Assembly.Location)!;
		AddAssemblyCache(root);
		Log.Source.TraceInformation("Assembly cache {0}", s_cache.Count);
	}

	static void AddAssemblyCache(string root)
	{
		foreach (var path in Directory.EnumerateFiles(root, "*.dll", SearchOption.AllDirectories))
		{
			if (path.Contains("\\native\\"))
				continue;

			var key = Path.GetFileNameWithoutExtension(path);

			// null existing dupe
			if (!s_cache.TryAdd(key, path))
				s_cache[key] = null;
		}
	}

	static string AssemblyNameToDllName(string name)
	{
		return name[..name.IndexOf(',')] + ".dll";
	}

	static bool IsRoot(string name)
	{
		return (name.Contains("\\FarNet\\Modules") || name.Contains("\\FarNet\\Lib")) && !name.Contains("\\runtimes");
	}

	static void AddRoot(string name)
	{
		if (s_lastRoots.First?.Value == name)
			return;

		s_lastRoots.Remove(name);
		s_lastRoots.AddFirst(name);
		while (s_lastRoots.Count > MaxLastRoots)
			s_lastRoots.RemoveLast();
	}

	static Assembly? ResolvePowerShellFar(string root, ResolveEventArgs args)
	{
		var caller = args.RequestingAssembly!.FullName!;
		var dllName = AssemblyNameToDllName(args.Name);

		if (caller.StartsWith("System.Management.Automation") || caller.StartsWith("PowerShellFar"))
		{
			// most frequent
			// PowerShellFar ->
			//   System.Management.Automation
			// System.Management.Automation ->
			//   Microsoft.PowerShell.ConsoleHost
			//   Microsoft.PowerShell.Commands.Utility
			//   Microsoft.PowerShell.Commands.Management
			//   Microsoft.PowerShell.Security
			var path = root + "\\runtimes\\win\\lib\\net6.0\\" + dllName;
			if (File.Exists(path))
				return Assembly.LoadFrom(path);
		}

		if (caller.StartsWith("System.Management.Automation"))
		{
			// System.Management.Automation ->
			//   System.Management
			var path = root + "\\" + dllName;
			if (File.Exists(path))
				return Assembly.LoadFrom(path);
		}

		if (caller.StartsWith("Microsoft.PowerShell.Commands.Management"))
		{
			// Microsoft.PowerShell.Commands.Management ->
			//   Microsoft.Management.Infrastructure
			var win10_x64 = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
			var path = root + "\\runtimes\\" + win10_x64 + "\\lib\\netstandard1.6\\" + dllName;
			if (File.Exists(path))
				return Assembly.LoadFrom(path);
		}

		return null;
	}

	// examples used to have issues:
	// 1) Microsoft.Bcl.AsyncInterfaces in Lib\FarNet.CsvHelper, Modules\PowerShellFar
	// 2) System.Data.OleDb.dll in Lib\FarNet.FSharp.Charting (2) Modules\FolderChart (2) Modules\PowerShellFar (2)
	public static Assembly? ResolveAssembly(string name, ResolveEventArgs args)
	{
		Log.Source.TraceInformation("LoadFrom {0}", name);

		// skip missing in FarNet
		if (!s_cache.TryGetValue(name, out object? value))
			return null;

		// unique in FarNet, load once
		if (value is not null)
		{
			// not yet loaded assembly path?
			if (value is not Assembly assembly)
			{
				assembly = Assembly.LoadFrom((string)value);
				s_cache[name] = assembly;
			}

			var location = assembly.Location;
			if (IsRoot(location))
				AddRoot(Path.GetDirectoryName(location)!);

			return assembly;
		}

		string? dllName = null;
		if (!args.RequestingAssembly!.IsDynamic)
		{
			var callerFile = args.RequestingAssembly.Location;

			// case: PowerShellFar
			int index = callerFile.LastIndexOf("\\PowerShellFar\\");
			if (index > 0)
				return ResolvePowerShellFar(callerFile[..(index + 14)], args);

			// case: same folder as the caller
			var callerRoot = Path.GetDirectoryName(callerFile)!;
			dllName = AssemblyNameToDllName(args.Name);
			var path = callerRoot + "\\" + dllName;
			if (File.Exists(path))
			{
				if (IsRoot(callerRoot))
					AddRoot(callerRoot);

				return Assembly.LoadFrom(path);
			}
		}

		// case: same folder as last roots
		dllName ??= AssemblyNameToDllName(args.Name);
		foreach(var root in s_lastRoots)
		{
			var path = root + "\\" + dllName;
			if (File.Exists(path))
			{
				AddRoot(root);
				return Assembly.LoadFrom(path);
			}
		}

		Log.Source.TraceInformation("Cannot load {0}", name);
		return null;
	}
}
