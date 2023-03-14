
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
	static readonly Dictionary<string, string> s_cache = new(StringComparer.OrdinalIgnoreCase);

	static AssemblyResolver()
	{
		var root = $"{Environment.GetEnvironmentVariable("FARHOME")}\\FarNet\\";
		AddAssemblyCache(root + "Modules");
		AddAssemblyCache(root + "Lib");
		Log.Source.TraceInformation("Assembly cache {0}", s_cache.Count);
	}

	static void AddAssemblyCache(string root)
	{
		if (!Directory.Exists(root))
			return;

		foreach (var path in Directory.EnumerateFiles(root, "*.dll", SearchOption.AllDirectories))
		{
			if (path.Contains("\\native\\") ||
				path.Contains("\\unix\\") ||
				path.EndsWith(".resources.dll"))
				continue;

			var key = Path.GetFileNameWithoutExtension(path);

			if (s_cache.TryGetValue(key, out string? value))
				s_cache[key] = $"{value}|{path}";
			else
				s_cache.Add(key, path);
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
			var path = root + "\\runtimes\\win\\lib\\net7.0\\" + dllName;
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

	static int GetSamePrefixLength(string path1, string path2)
	{
		int index = 0;
		while (index < path1.Length && index < path2.Length && char.ToUpperInvariant(path1[index]) == char.ToUpperInvariant(path2[index]))
			++index;
		return index;
	}

	static string FindBestPath(string path, string[] paths)
	{
		string? bestPath = null;
		int maxPrefixLength = -1;
		foreach (var path2 in paths)
		{
			int length = GetSamePrefixLength(path, path2);
			if (length > maxPrefixLength)
			{
				bestPath = path2;
				maxPrefixLength = length;
			}
		}
		return bestPath!;
	}

	static Assembly? LoadFromLastRoots(string dllName)
	{
		foreach (var root in s_lastRoots)
		{
			var path = root + "\\" + dllName;
			if (File.Exists(path))
			{
				AddRoot(root);
				return Assembly.LoadFrom(path);
			}
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
		if (!s_cache.TryGetValue(name, out string? pathsString))
			return null;

		// unique in FarNet, load
		var paths = pathsString.Split('|');
		if (paths.Length == 1)
		{
			var location = paths[0];
			var assembly = Assembly.LoadFrom(location);

			if (IsRoot(location))
				AddRoot(Path.GetDirectoryName(location)!);

			return assembly;
		}

		// 2+ candidates exist
		// how to test:
		// Microsoft.CodeAnalysis issue
		// - InferKit scripts should work
		// - Test\Debugger\Debug-Assert-Far-1.fas.ps1 should work
		string? dllName = null;
		if (!args.RequestingAssembly!.IsDynamic)
		{
			var callerLocation = args.RequestingAssembly.Location;

			// case: PowerShellFar
			// why? PowerShell package is very convoluted and unpredictable, we hard code some known fixes
			int index = callerLocation.LastIndexOf("\\PowerShellFar\\");
			if (index > 0)
			{
				var assembly = ResolvePowerShellFar(callerLocation[..(index + 14)], args);
				if (assembly != null)
					return assembly;
			}

			// try: same folder as last roots
			// why before best? weird, but on running InferKit scripts on loading
			// Microsoft.CodeAnalysis the RequestingAssembly is one of PowerShell
			// instead of some InferKit assembly
			dllName ??= AssemblyNameToDllName(args.Name);
			{
				var assembly = LoadFromLastRoots(dllName);
				if (assembly != null)
					return assembly;
			}

			// finally: use the best candidate
			var location = FindBestPath(callerLocation, paths);
			if (IsRoot(location))
				AddRoot(Path.GetDirectoryName(location)!);

			return Assembly.LoadFrom(location);
		}

		// try: same folder as last roots
		dllName ??= AssemblyNameToDllName(args.Name);
		{
			var assembly = LoadFromLastRoots(dllName);
			if (assembly != null)
				return assembly;
		}

		Log.Source.TraceInformation("Cannot load {0}", name);
		return null;
	}
}
