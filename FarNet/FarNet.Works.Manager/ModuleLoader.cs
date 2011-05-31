
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

namespace FarNet.Works
{
	public class ModuleLoader
	{
		static readonly SortedList<string, ModuleManager> _Managers = new SortedList<string, ModuleManager>(StringComparer.OrdinalIgnoreCase);
		readonly ModuleCache _Cache;
		///
		public ModuleLoader()
		{
			// read the cache
			_Cache = new ModuleCache();
		}
		/// <summary>
		/// Loads modules from the root directory.
		/// </summary>
		/// <param name="rootPath">The root module directory path.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void LoadModules(string rootPath)
		{
			// directories
			foreach (string dir in Directory.GetDirectories(rootPath))
			{
				try
				{
					LoadModule(dir + "\\" + Path.GetFileName(dir) + ".dll");
				}
				catch (Exception ex)
				{
					Far.Net.ShowError("Error on loading " + dir, ex);
				}
			}

			// write the cache
			_Cache.Update();
		}
		/// <summary>
		/// Loads the module assembly.
		/// </summary>
		/// <param name="fileName">The assembly path to load a module from.</param>
		void LoadModule(string fileName)
		{
			Log.Source.TraceInformation("Load module {0}", fileName);

			// use the file info to reduce file access
			var fileInfo = new FileInfo(fileName);
			if (!fileInfo.Exists)
			{
				Log.Source.TraceInformation("Module is not found.");
				return;
			}

			// load from the cache
			if (ReadCache(fileInfo))
				return;

			// add new module manager now, it will be removed on errors
			ModuleManager manager = new ModuleManager(fileInfo.FullName);
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
				foreach (Type type in assembly.GetExportedTypes())
				{
					if (typeof(BaseModuleItem).IsAssignableFrom(type) && !type.IsAbstract)
						actionCount += LoadType(manager, settings, type);
					else if (!manager.HasSettings && typeof(ApplicationSettingsBase).IsAssignableFrom(type) && !type.IsAbstract)
						manager.HasSettings = true;
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
		/// Reads the module from the cache.
		/// </summary>
		/// <param name="fileInfo">Module file information.</param>
		/// <returns>True if the module has been loaded from the cache.</returns>
		bool ReadCache(FileInfo fileInfo)
		{
			Log.Source.TraceInformation("Read cache {0}", fileInfo);

			string path = fileInfo.FullName;
			var data = _Cache.Get(path);
			if (data == null)
				return false;

			++_Cache.CountLoaded;
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

					// remove the manager
					if (manager != null)
						RemoveModuleManager(manager);
				}
			}

			return done;
		}
		/// <summary>
		/// Loads the module item by type.
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
		/// Saves the module cache.
		/// </summary>
		void SaveModuleCache(ModuleManager manager, FileInfo fileInfo)
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
				data.Add((int)ModuleItemKind.Host);
				// Class
				data.Add(hostClassName);
			}

			// << Actions
			foreach (ProxyAction it in Host.Actions.Values)
				if (it.Manager == manager)
					it.WriteCache(data);

			// to write
			_Cache.Set(manager.AssemblyPath, data);
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
