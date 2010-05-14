/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace FarNet.Works
{
	public static class ModuleLoader
	{
		const string CacheVersion = "10";

		static readonly SortedList<string, ModuleManager> _Managers = new SortedList<string, ModuleManager>();

		/// <summary>
		/// Loads all modules from the root directory.
		/// </summary>
		/// <param name="rootPath">The root directory path.</param>
		public static void LoadModules(string rootPath)
		{
			// directories:
			foreach (string dir in Directory.GetDirectories(rootPath))
			{
				// load not disabled
				if (!Path.GetFileName(dir).StartsWith("-", StringComparison.Ordinal))
					LoadDirectory(dir);
			}
		}

		/// <summary>
		/// #1 Loads a module from the directory.
		/// </summary>
		/// <param name="directoryPath">The directory path to load a module from.</param>
		/// <remarks>
		/// Directories with no .CFG or .DLL files (not yet built sources) are simply ignored.
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static void LoadDirectory(string directoryPath)
		{
			Log.Source.TraceInformation("Load directory {0}", directoryPath);
			
			try
			{
				// use the manifest if any
				string[] manifests = Directory.GetFiles(directoryPath, "*.CFG");
				if (manifests.Length == 1)
				{
					LoadManifest(manifests[0], directoryPath);
					return;
				}
				
				// fail on 2+ manifest files
				if (manifests.Length > 1)
					throw new ModuleException("More than one .CFG files found.");

				// use the assembly
				string[] assemblies = Directory.GetFiles(directoryPath, "*.DLL");
				if (assemblies.Length == 1)
				{
					LoadAssembly(assemblies[0], null);
					return;
				}

				// fail on 2+ assembly files
				if (assemblies.Length > 1)
					throw new ModuleException("More than one .DLL files found. Expected exactly one .DLL file or exactly one .CFG file.");
			}
			catch (Exception ex)
			{
				Far.Net.ShowError("ERROR: directory " + directoryPath, ex);
			}
		}

		/// <summary>
		/// #2 Loads the manifest.
		/// </summary>
		/// <param name="filePath">The manifest file path.</param>
		/// <param name="directoryPath">The root directory for relative paths.</param>
		/// <remarks>
		/// For now assume that manifests contain relative paths.
		/// </remarks>
		static void LoadManifest(string filePath, string directoryPath)
		{
			Log.Source.TraceInformation("Load manifest {0}", filePath);

			string[] lines = File.ReadAllLines(filePath);
			if (lines.Length == 0)
				throw new ModuleException("The manifest file is empty.");

			// assembly
			string path = lines[0].TrimEnd();
			if (path.Length == 0)
				throw new ModuleException("Expected the module assembly name as the first line of the manifest file.");
			path = Path.Combine(directoryPath, path);

			// collect classes
			var classes = new List<string>(lines.Length - 1);
			for (int i = 1; i < lines.Length; ++i)
			{
				string name = lines[i].Trim();
				if (name.Length > 0)
					classes.Add(name);
			}

			// load with classes, if any
			LoadAssembly(path, classes);
		}

		/// <summary>
		/// #3 Loads the assembly.
		/// </summary>
		/// <param name="assemblyPath">The assembly path to load a module from.</param>
		/// <param name="classes">Optional predefined classes.</param>
		static void LoadAssembly(string assemblyPath, List<string> classes)
		{
			Log.Source.TraceInformation("Load assembly {0}", assemblyPath);

			// load from the cache
			FileInfo fileInfo = new FileInfo(assemblyPath);
			if (ReadCache(fileInfo))
				return;

			// add new module manager now, it will be removed on errors
			ModuleManager manager = new ModuleManager(assemblyPath);
			_Managers.Add(fileInfo.Name, manager);
			bool done = false;
			try
			{
				Log.Source.TraceInformation("Load module {0}", manager.ModuleName);

				int actionCount = 0;
				Assembly assembly = manager.AssemblyInstance;
				if (classes != null && classes.Count > 0)
				{
					foreach (string name in classes)
						actionCount += LoadType(manager, assembly.GetType(name, true));
				}
				else
				{
					foreach (Type type in assembly.GetExportedTypes())
					{
						if (!type.IsAbstract && typeof(BaseModuleItem).IsAssignableFrom(type))
							actionCount += LoadType(manager, type);
					}
				}

				// if the module has the host to load then load it now, if it is not loaded then the module should be cached
				if (!manager.LoadLoadableModuleHost())
				{
					if (0 == actionCount)
						throw new ModuleException("A module must have a public action or a pre-loadable host.");

					SaveModuleCache(manager, fileInfo);
				}

				// done
				done = true;
			}
			finally
			{
				if (!done)
					RemoveModuleManager(manager);
			}
		}

		/// <summary>
		/// #4 Reads the module from the cache.
		/// </summary>
		/// <param name="fileInfo">Module file information.</param>
		/// <returns>True if the module has been loaded from the cache.</returns>
		static bool ReadCache(FileInfo fileInfo)
		{
			Log.Source.TraceInformation("Read cache {0}", fileInfo);

			// open for writing, to remove obsolete data
			using (IRegistryKey cache = Host.Instance.OpenCacheKey(true))
			{
				// get cached data
				string fullName = fileInfo.FullName;
				object data = cache.GetValue(fullName, null);
				if (data == null)
					return false;

				bool done = false;
				ModuleManager manager = null;
				try
				{
					// read data
					EnumerableReader reader = new EnumerableReader((string[])data);

					// Version
					string version = reader.Read();
					if (version != CacheVersion)
						return false;

					// Stamp
					string assemblyStamp = reader.Read();
					if (assemblyStamp != fileInfo.LastWriteTime.Ticks.ToString(CultureInfo.InvariantCulture))
						return false;

					// new manager, add it now, remove later on errors
					manager = new ModuleManager(fullName);
					_Managers.Add(manager.ModuleName, manager);

					// culture of cached resources
					string savedCulture = reader.Read();

					// check the culture
					if (savedCulture.Length > 0)
					{
						// the culture changed, ignore the cache
						if (savedCulture != manager.CurrentUICulture.Name)
							return false;

						// restore the flag
						manager.CachedResources = true;
					}

					string kindText;
					while (null != (kindText = reader.TryRead()))
					{
						ModuleItemKind kind = (ModuleItemKind)Enum.Parse(typeof(ModuleItemKind), kindText);
						switch (kind)
						{
							case ModuleItemKind.Host:
								manager.SetModuleHost(reader.Read());
								break;
							case ModuleItemKind.Command:
								Host.Instance.RegisterProxyCommand(new ProxyCommand(manager, reader));
								break;
							case ModuleItemKind.Editor:
								Host.Instance.RegisterProxyEditor(new ProxyEditor(manager, reader));
								break;
							case ModuleItemKind.Filer:
								Host.Instance.RegisterProxyFiler(new ProxyFiler(manager, reader));
								break;
							case ModuleItemKind.Tool:
								Host.Instance.RegisterProxyTool(new ProxyTool(manager, reader));
								break;
							default:
								throw new ModuleException();
						}
					}

					done = true;
				}
				catch (ModuleException)
				{
					// ignore known
				}
				catch (Exception ex)
				{
					throw new ModuleException(Invariant.Format("Error on reading the cache from '{0}'.", cache), ex);
				}
				finally
				{
					if (!done)
					{
						// remove cached data
						cache.SetValue(fileInfo.FullName, null);
						
						// remove the manager
						if (manager != null)
							RemoveModuleManager(manager);
					}
				}

				return done;
			}
		}

		/// <summary>
		/// #5 Loads the module item by type.
		/// </summary>
		static int LoadType(ModuleManager manager, Type type)
		{
			Log.Source.TraceInformation("Load class {0}", type);

			// case: host
			if (typeof(ModuleHost).IsAssignableFrom(type))
			{
				manager.SetModuleHost(type);
				return 0;
			}

			// command
			if (typeof(ModuleCommand).IsAssignableFrom(type))
				Host.Instance.RegisterProxyCommand(new ProxyCommand(manager, type));
			// editor
			else if (typeof(ModuleEditor).IsAssignableFrom(type))
				Host.Instance.RegisterProxyEditor(new ProxyEditor(manager, type));
			// filer
			else if (typeof(ModuleFiler).IsAssignableFrom(type))
				Host.Instance.RegisterProxyFiler(new ProxyFiler(manager, type));
			// tool
			else if (typeof(ModuleTool).IsAssignableFrom(type))
				Host.Instance.RegisterProxyTool(new ProxyTool(manager, type));
			else
				throw new ModuleException("Unknown module class type.");

			return 1;
		}

		/// <summary>
		/// #6 Saves the module cache.
		/// </summary>
		static void SaveModuleCache(ModuleManager manager, FileInfo fileInfo)
		{
			Log.Source.TraceInformation("Save cache {0}", fileInfo);

			using (IRegistryKey cache = Host.Instance.OpenCacheKey(true))
			{
				var data = new List<string>();

				// Version
				data.Add(CacheVersion);

				// Stamp
				data.Add(fileInfo.LastWriteTime.Ticks.ToString(CultureInfo.InvariantCulture));

				// Culture
				if (manager.CachedResources)
					data.Add(manager.CurrentUICulture.Name);
				else
					data.Add(string.Empty);

				// host
				string hostClassName = manager.GetModuleHostClassName();
				if (hostClassName != null)
				{
					// Type
					data.Add("Host");
					// Class
					data.Add(hostClassName);
				}

				// write actions of the manager
				foreach (ProxyAction it in Host.Actions.Values)
					if (it.Manager == manager)
						it.WriteCache(data);

				// write to the registry
				cache.SetValue(manager.AssemblyPath, data.ToArray());
			}
		}

		public static bool CanExit()
		{
			foreach (ModuleManager manager in _Managers.Values)
			{
				if (manager.GetLoadedModuleHost() != null && !manager.GetLoadedModuleHost().CanExit())
					return false;
			}

			return true;
		}

		public static IList<IModuleManager> GatherModuleManagers()
		{
			var result = new List<IModuleManager>(_Managers.Count);
			foreach (ModuleManager it in _Managers.Values)
				result.Add(it);
			return result;
		}

		//! Don't use Far UI
		internal static void RemoveModuleManager(ModuleManager manager)
		{
			// remove the module
			_Managers.Remove(manager.ModuleName);

			// 1) gather its actions
			var actions = new List<ProxyAction>();
			foreach (ProxyAction action in Host.Actions.Values)
				if (action.Manager == manager)
					actions.Add(action);

			// 2) unregister its actions
			foreach (ProxyAction action in actions)
				action.Unregister();
		}

		//! Don't use Far UI
		public static void UnloadModules()
		{
			// unregister managers
			while (_Managers.Count > 0)
				_Managers.Values[0].Unregister();

			// actions are removed
			Debug.Assert(Host.Actions.Count == 0);
		}

	}
}
