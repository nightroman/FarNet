
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2011 FarNet Team
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace FarNet.Works
{
	public static class ModuleLoader
	{
		const int idVersion = 0;
		const int CacheVersion = 1;
		static readonly SortedList<string, ModuleManager> _Managers = new SortedList<string, ModuleManager>();
		static Hashtable _Cache;
		static int _CacheLoaded;
		static bool _CacheUpdate;
		/// <summary>
		/// Loads all modules from the root directory.
		/// </summary>
		/// <param name="rootPath">The root directory path.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static void LoadModules(string rootPath)
		{
			// read the cache
			string path = Far.Net.GetFolderPath(SpecialFolder.LocalData) + @"\FarNet\Cache.binary";
			if (File.Exists(path))
			{
				try
				{
					object deserialized;
					var formatter = new BinaryFormatter();
					using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
						deserialized = formatter.Deserialize(stream);

					_Cache = deserialized as Hashtable;
					
					if (_Cache != null && CacheVersion != (int)_Cache[idVersion])
						_Cache = null;
				}
				catch (Exception ex)
				{
					_Cache = null;
					Far.Net.ShowError("Reading cache", ex);
				}
			}

			// new empty cache
			if (_Cache == null)
			{
				//_Cache = new Hashtable(StringComparer.OrdinalIgnoreCase);
				_Cache = new Hashtable();
				_Cache.Add(idVersion, CacheVersion);
			}

			// count to load
			int toLoad = _Cache.Count - 1;

			// directories:
			foreach (string dir in Directory.GetDirectories(rootPath))
			{
				// load not disabled
				if (!Path.GetFileName(dir).StartsWith("-", StringComparison.Ordinal))
					LoadDirectory(dir);
			}

			// obsolete records? 
			if (toLoad != _CacheLoaded)
			{
				var list = new List<string>();

				foreach (var key in _Cache.Keys)
				{
					var name = key as string;
					if (name != null && !File.Exists(name))
						list.Add(name);
				}

				if (list.Count > 0)
				{
					_CacheUpdate = true;
					foreach (var name in list)
						_Cache.Remove(name);
				}
			}

			// write cache
			if (_CacheUpdate)
			{
				try
				{
					// ensure the directory
					var dir = Path.GetDirectoryName(path);
					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);

					// write the cache
					var formatter = new BinaryFormatter();
					using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
						formatter.Serialize(stream, _Cache);
				}
				catch (Exception ex)
				{
					Far.Net.ShowError("Writing cache", ex);
				}
			}

			// done with the cache
			_Cache = null;
		}
		/// <summary>
		/// #1 Loads a module from the directory.
		/// </summary>
		/// <param name="directoryPath">The directory path to load a module from.</param>
		/// <remarks>
		/// Directories with no .CFG or .DLL files (not yet built sources) are simply ignored.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
			_Managers.Add(manager.ModuleName, manager);

			// read and load data
			var settings = manager.ReadSettings();
			manager.LoadData(settings);

			bool done = false;
			try
			{
				Log.Source.TraceInformation("Load module {0}", manager.ModuleName);

				int actionCount = 0;
				Assembly assembly = manager.LoadAssembly();
				if (classes != null && classes.Count > 0)
				{
					foreach (string name in classes)
						actionCount += LoadType(manager, settings, assembly.GetType(name, true));
				}
				else
				{
					foreach (Type type in assembly.GetExportedTypes())
					{
						if (typeof(BaseModuleItem).IsAssignableFrom(type) && !type.IsAbstract)
							actionCount += LoadType(manager, settings, type);
						else if (!manager.HasSettings && typeof(ApplicationSettingsBase).IsAssignableFrom(type) && !type.IsAbstract)
							manager.HasSettings = true;
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

			string path = fileInfo.FullName;
			var data = _Cache[path];
			if (data == null)
				return false;

			++_CacheLoaded;
			bool done = false;
			ModuleManager manager = null;
			try
			{
				// read data
				EnumerableReader reader = new EnumerableReader((IEnumerable)data);

				// >> Stamp
				var assemblyStamp = (long)reader.Read();
				if (assemblyStamp != fileInfo.LastWriteTime.Ticks)
					return false;

				// new manager, add it now, remove later on errors
				manager = new ModuleManager(path);
				_Managers.Add(manager.ModuleName, manager);

				// read and load data
				var settings = manager.ReadSettings();
				manager.LoadData(settings);

				// >> Culture of cached resources
				var savedCulture = (string)reader.Read();

				// check the culture
				if (savedCulture.Length > 0)
				{
					// the culture changed, ignore the cache
					if (savedCulture != manager.CurrentUICulture.Name)
						return false;

					// restore the flag
					manager.CachedResources = true;
				}

				// >> Settings
				manager.HasSettings = (bool)reader.Read();

				object kind;
				while (null != (kind = reader.TryRead()))
				{
					ProxyAction action = null;
					switch ((ModuleItemKind)kind)
					{
						case ModuleItemKind.Host:
							{
								manager.SetModuleHost((string)reader.Read());
							}
							break;
						case ModuleItemKind.Command:
							{
								var it = new ProxyCommand(manager, reader);
								Host.Instance.RegisterProxyCommand(it);
								action = it;
							}
							break;
						case ModuleItemKind.Editor:
							{
								var it = new ProxyEditor(manager, reader);
								Host.Instance.RegisterProxyEditor(it);
								action = it;
							}
							break;
						case ModuleItemKind.Filer:
							{
								var it = new ProxyFiler(manager, reader);
								Host.Instance.RegisterProxyFiler(it);
								action = it;
							}
							break;
						case ModuleItemKind.Tool:
							{
								var it = new ProxyTool(manager, reader);
								Host.Instance.RegisterProxyTool(it);
								action = it;
							}
							break;
						default:
							throw new ModuleException();
					}

					if (action != null)
						action.LoadData((Hashtable)settings[action.Id]);
				}

				done = true;
			}
			catch (ModuleException)
			{
				// ignore known
			}
			catch (Exception ex)
			{
				throw new ModuleException(string.Format(null, "Error on reading the cache for '{0}'.", path), ex);
			}
			finally
			{
				if (!done)
				{
					// remove cached data
					_Cache.Remove(path);
					_CacheUpdate = true;

					// remove the manager
					if (manager != null)
						RemoveModuleManager(manager);
				}
			}

			return done;
		}
		/// <summary>
		/// #5 Loads the module item by type.
		/// </summary>
		static int LoadType(ModuleManager manager, Hashtable settings, Type type)
		{
			Log.Source.TraceInformation("Load class {0}", type);

			// case: host
			if (typeof(ModuleHost).IsAssignableFrom(type))
			{
				manager.SetModuleHost(type);
				return 0;
			}

			// case: settings
			if (typeof(ApplicationSettingsBase).IsAssignableFrom(type))
			{
				manager.HasSettings = true;
				return 0;
			}

			// command
			ProxyAction action;
			if (typeof(ModuleCommand).IsAssignableFrom(type))
			{
				var it = new ProxyCommand(manager, type);
				Host.Instance.RegisterProxyCommand(it);
				action = it;
			}
			// editor
			else if (typeof(ModuleEditor).IsAssignableFrom(type))
			{
				var it = new ProxyEditor(manager, type);
				Host.Instance.RegisterProxyEditor(it);
				action = it;
			}
			// filer
			else if (typeof(ModuleFiler).IsAssignableFrom(type))
			{
				var it = new ProxyFiler(manager, type);
				Host.Instance.RegisterProxyFiler(it);
				action = it;
			}
			// tool
			else if (typeof(ModuleTool).IsAssignableFrom(type))
			{
				var it = new ProxyTool(manager, type);
				Host.Instance.RegisterProxyTool(it);
				action = it;
			}
			else
				throw new ModuleException("Unknown module class type.");

			// set settings
			action.LoadData((Hashtable)settings[action.Id]);

			return 1;
		}
		/// <summary>
		/// #6 Saves the module cache.
		/// </summary>
		static void SaveModuleCache(ModuleManager manager, FileInfo fileInfo)
		{
			Log.Source.TraceInformation("Save cache {0}", fileInfo);

			var data = new ArrayList();

			// << Stamp
			data.Add(fileInfo.LastWriteTime.Ticks);

			// << Culture
			if (manager.CachedResources)
				data.Add(manager.CurrentUICulture.Name);
			else
				data.Add(string.Empty);

			// << Settings
			data.Add(manager.HasSettings);

			// << Host
			string hostClassName = manager.GetModuleHostClassName();
			if (hostClassName != null)
			{
				// Type
				data.Add(ModuleItemKind.Host);
				// Class
				data.Add(hostClassName);
			}

			// << Actions
			foreach (ProxyAction it in Host.Actions.Values)
				if (it.Manager == manager)
					it.WriteCache(data);

			// to write
			_Cache[manager.AssemblyPath] = data;
			_CacheUpdate = true;
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
		public static IEnumerable<IModuleManager> EnumSettings()
		{
			foreach (ModuleManager it in _Managers.Values)
				if (it.HasSettings)
					yield return it;
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
		public static IModuleManager GetModuleManager(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			var name = Path.GetFileNameWithoutExtension(type.Assembly.Location);
			return _Managers[name];
		}
	}
}
