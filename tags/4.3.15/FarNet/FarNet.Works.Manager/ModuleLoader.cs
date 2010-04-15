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
		const string CacheVersion = "9";
		static bool ToCacheVersion;

		static readonly SortedList<string, ModuleManager> _Managers = new SortedList<string, ModuleManager>();

		// #1 Load all
		public static void LoadModules(string root)
		{
			// read modules from the cache, up-to-date modules get loaded with static info
			ReadModuleCache();

			// read from module directories:
			foreach (string dir in Directory.GetDirectories(root))
			{
				// skip
				if (Path.GetFileName(dir).StartsWith("-", StringComparison.Ordinal))
					continue;

				// load
				LoadFromDirectory(dir);
			}
		}

		// #2 Read cache
		static void ReadModuleCache()
		{
			using (Log log = Log.Switch.TraceInfo ? new Log("Read module cache") : null)
			{
				// open for writing, to remove obsolete data
				using (IRegistryKey cache = Host.Instance.OpenCacheKey(true))
				{
					// different version: drop cache values
					string version = cache.GetValue(string.Empty, string.Empty).ToString();
					if (version != CacheVersion)
					{
						foreach (string name in cache.GetValueNames())
							cache.SetValue(name, null);

						ToCacheVersion = true;
						return;
					}

					// process cache values
					foreach (string assemblyPath in cache.GetValueNames())
					{
						if (assemblyPath.Length == 0)
							continue;

						bool done = false;
						ModuleManager manager = null;
						try
						{
							// exists?
							if (!File.Exists(assemblyPath))
								throw new ModuleException();

							// read data
							EnumerableReader reader = new EnumerableReader((string[])cache.GetValue(assemblyPath, null));

							// Stamp
							string assemblyStamp = reader.Read();
							FileInfo fi = new FileInfo(assemblyPath);

							// stamp mismatch: do not throw!
							if (assemblyStamp != fi.LastWriteTime.Ticks.ToString(CultureInfo.InvariantCulture))
								continue;

							// new manager, add it now, remove later on errors
							manager = new ModuleManager(assemblyPath);
							_Managers.Add(manager.ModuleName, manager);

							// culture of cached resources
							string savedCulture = reader.Read();

							// check the culture
							if (savedCulture.Length > 0)
							{
								// the culture changed, ignore the cache
								if (savedCulture != manager.CurrentUICulture.Name)
									continue;

								// restore the flag
								manager.CachedResources = true;
							}

							for (; ; )
							{
								// Kind, can be end of data
								string kindText = reader.TryRead();
								if (kindText == null)
									break;

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
								cache.SetValue(assemblyPath, null);
								if (manager != null)
									RemoveModuleManager(manager);
							}
						}
					}
				}
			}
		}

		// #3
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static void LoadFromDirectory(string dir)
		{
			try
			{
				// the manifest
				string[] manifests = Directory.GetFiles(dir, "*.cfg");
				if (manifests.Length == 1)
				{
					LoadFromManifest(manifests[0], dir);
					return;
				}
				if (manifests.Length > 1)
					throw new ModuleException("More than one .cfg files found.");

				// load the only assembly
				string[] assemblies = Directory.GetFiles(dir, "*.dll");
				if (assemblies.Length == 1)
					LoadFromAssembly(assemblies[0], null);
				else if (assemblies.Length > 1)
					throw new ModuleException("More than one .dll files found. Expected exactly one .dll file or exactly one .cfg file.");

				//! If the folder has no .dll or .cfg files (not yet built sources) then just ignore
			}
			catch (Exception ex)
			{
				Far.Net.ShowError("ERROR: module " + dir, ex);
			}
		}

		// #4
		static void LoadFromManifest(string file, string dir)
		{
			string[] lines = File.ReadAllLines(file);
			if (lines.Length == 0)
				throw new ModuleException("The manifest file is empty.");

			// assembly
			string path = lines[0].TrimEnd();
			if (path.Length == 0)
				throw new ModuleException("Expected the module assembly name as the first line of the manifest file.");
			path = Path.Combine(dir, path);

			// collect classes
			var classes = new List<string>(lines.Length - 1);
			for (int i = 1; i < lines.Length; ++i)
			{
				string name = lines[i].Trim();
				if (name.Length > 0)
					classes.Add(name);
			}

			// load with classes, if any
			LoadFromAssembly(path, classes);
		}

		// #5 Loads the assembly, writes cache
		static void LoadFromAssembly(string assemblyPath, List<string> classes)
		{
			// the name
			string assemblyName = Path.GetFileName(assemblyPath);

			// already loaded (normally from cache)?
			if (_Managers.ContainsKey(assemblyName))
				return;

			// add new module manager now, it will be removed on errors
			ModuleManager manager = new ModuleManager(assemblyPath);
			_Managers.Add(assemblyName, manager);
			bool done = false;
			try
			{
				using (Log log = Log.Switch.TraceInfo ? new Log("Load module " + manager.ModuleName) : null)
				{
					int actionCount = 0;
					Assembly assembly = manager.AssemblyInstance;
					if (classes != null && classes.Count > 0)
					{
						foreach (string name in classes)
							actionCount += LoadClass(manager, assembly.GetType(name, true));
					}
					else
					{
						foreach (Type type in assembly.GetExportedTypes())
						{
							if (!type.IsAbstract && typeof(BaseModuleItem).IsAssignableFrom(type))
								actionCount += LoadClass(manager, type);
						}
					}

					// if the module has the host to load then load it now, if it is not loaded then the module should be cached
					if (!manager.LoadLoadableModuleHost())
					{
						if (0 == actionCount)
							throw new ModuleException("The module must implement a public action or a preloadable host.");

						WriteModuleCache(manager);
					}

					// done
					done = true;
				}
			}
			finally
			{
				if (!done)
					RemoveModuleManager(manager);
			}
		}

		// #6 Adds a module item
		static int LoadClass(ModuleManager manager, Type type)
		{
			using (Log log = Log.Switch.TraceInfo ? new Log("Load class " + type) : null)
			{
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

		static void WriteModuleCache(ModuleManager manager)
		{
			using (IRegistryKey cache = Host.Instance.OpenCacheKey(true))
			{
				// update cache version
				if (ToCacheVersion)
				{
					ToCacheVersion = false;
					cache.SetValue(string.Empty, CacheVersion.ToString());
				}

				FileInfo fi = new FileInfo(manager.AssemblyPath);
				var data = new List<string>();

				// Stamp
				data.Add(fi.LastWriteTime.Ticks.ToString(CultureInfo.InvariantCulture));

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
	}
}
