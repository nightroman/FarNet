using System.Diagnostics;
using System.Reflection;

namespace FarNet.Works;
#pragma warning disable 1591

public static class AssemblyResolver
{
	const int MaxLastRoots = 4;
	const string Win64 = "win-x64";
	const string Win86 = "win-x86";

	static readonly string[] _folders;
	static readonly LinkedList<string> _lastRoots = [];

	static AssemblyResolver()
	{
		var root = $"{Environment.GetEnvironmentVariable("FARHOME")}\\FarNet\\";
		var folders = new List<string>(2);
		{
			var dir = root + "Modules";
			if (Directory.Exists(dir))
				folders.Add(dir);
		}
		{
			var dir = root + "Lib";
			if (Directory.Exists(dir))
				folders.Add(dir);
		}
		_folders = [.. folders];
	}

	static bool IsRoot(string name)
	{
		return (name.Contains("\\FarNet\\Modules") || name.Contains("\\FarNet\\Lib")) && !name.Contains("\\runtimes");
	}

	static void AddRoot(string name)
	{
		if (_lastRoots.First?.Value == name)
			return;

		_lastRoots.Remove(name);
		_lastRoots.AddFirst(name);
		while (_lastRoots.Count > MaxLastRoots)
			_lastRoots.RemoveLast();
	}

	static Assembly? ResolvePowerShellFar(ReadOnlySpan<char> root, string dllName, string callerFullName)
	{
		if (callerFullName.StartsWith("System.Management.Automation") || callerFullName.StartsWith("PowerShellFar"))
		{
			// most frequent
			// PowerShellFar ->
			//   System.Management.Automation
			// System.Management.Automation ->
			//   Microsoft.PowerShell.ConsoleHost
			//   Microsoft.PowerShell.Commands.Utility
			//   Microsoft.PowerShell.Commands.Management
			//   Microsoft.PowerShell.Security
			var path = $"{root}\\runtimes\\win\\lib\\net9.0\\{dllName}";
			if (File.Exists(path))
				return Assembly.LoadFrom(path);
		}

		if (callerFullName.StartsWith("System.Management.Automation"))
		{
			// System.Management.Automation ->
			//   System.Management
			var path = $"{root}\\{dllName}";
			if (File.Exists(path))
				return Assembly.LoadFrom(path);
		}

		// Microsoft.PowerShell.Commands.Management ->
		// System.Management.Automation ->
		//   Microsoft.Management.Infrastructure
		{
			var path = $"{root}\\runtimes\\{Win64}\\lib\\netstandard1.6\\{dllName}";
			if (File.Exists(path))
				return Assembly.LoadFrom(path);
		}

		return null;
	}

	static int GetSamePrefixLength(string path1, ReadOnlySpan<char> path2)
	{
		int index = 0;
		while (index < path1.Length && index < path2.Length && char.ToUpperInvariant(path1[index]) == char.ToUpperInvariant(path2[index]))
			++index;
		return index;
	}

	static string FindBestPath(string path, List<string> paths)
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
		foreach (var root in _lastRoots)
		{
			var path = $"{root}\\{dllName}";
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
	public static Assembly? ResolveAssembly(ResolveEventArgs args)
	{
		// e.g. XmlSerializers
		if (args.RequestingAssembly is null)
			return null;

		var name = args.Name.AsSpan(0, args.Name.IndexOf(','));
		if (name.EndsWith(".resources"))
			return null;

		var dllName = $"{name}.dll";
		var paths = _folders.SelectMany(x => Directory.EnumerateFiles(x, dllName, SearchOption.AllDirectories)).ToList();

		// skip missing in FarNet
		if (paths.Count == 0)
			return null;

		Debug.WriteLine($"## ResolveAssembly {name} <-- {args.RequestingAssembly.Location}");

		if (paths.Count > 1)
			paths.RemoveAll(x => x.Contains(Win86));

		// one in FarNet
		if (paths.Count == 1)
		{
			var path = paths[0];
			var assembly = Assembly.LoadFrom(path);

			if (IsRoot(path))
				AddRoot(Path.GetDirectoryName(path)!);

			Debug.WriteLine($"## -> one {assembly.Location}");
			return assembly;
		}

		// 2+ candidates exist
		// how to test:
		// Microsoft.CodeAnalysis issue
		// - InferKit scripts should work
		// - Test\Debugger\Debug-Assert-Far-1.fas.ps1 should work
		if (!args.RequestingAssembly.IsDynamic)
		{
			var callerLocation = args.RequestingAssembly.Location;

			// case: PowerShellFar
			// why? PowerShell package is very convoluted and unpredictable, we hard code some known fixes
			int index = callerLocation.LastIndexOf("\\PowerShellFar\\");
			if (index > 0)
			{
				var assembly = ResolvePowerShellFar(callerLocation.AsSpan(0, index + 14), dllName, args.RequestingAssembly.FullName!);
				if (assembly is { })
				{
					Debug.WriteLine($"## -> psf {assembly.Location}");
					return assembly;
				}
			}

			// try: same folder as last roots
			// why before best? weird, but on running InferKit scripts on loading
			// Microsoft.CodeAnalysis the RequestingAssembly is one of PowerShell
			// instead of some InferKit assembly
			{
				var assembly = LoadFromLastRoots(dllName);
				if (assembly is { })
				{
					Debug.WriteLine($"## -> roots-1 {assembly.Location}");
					return assembly;
				}
			}

			// finally: use the best candidate
			var location = FindBestPath(callerLocation, paths);
			if (IsRoot(location))
				AddRoot(Path.GetDirectoryName(location)!);

			Debug.WriteLine($"## -> best-path {location}");
			return Assembly.LoadFrom(location);
		}

		// try: same folder as last roots
		{
			var assembly = LoadFromLastRoots(dllName);
			if (assembly is { })
			{
				Debug.WriteLine($"## -> roots-2 {assembly.Location}");
				return assembly;
			}
		}

		return null;
	}
}
