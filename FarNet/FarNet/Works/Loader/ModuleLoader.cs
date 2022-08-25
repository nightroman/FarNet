
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace FarNet.Works;
#pragma warning disable 1591

public class ModuleLoader
{
	static readonly SortedList<string, ModuleManager> _Managers = new(StringComparer.OrdinalIgnoreCase);
	ModuleCache _Cache;

	public ModuleLoader()
	{
		// read the cache
		_Cache = new ModuleCache();
	}

	// Loads modules from the root directory.
	public void LoadModules(string rootPath)
	{
		// config
		var config = Config.Default.GetData();

		// directories
		foreach (string dir in Directory.GetDirectories(rootPath))
		{
			try
			{
				LoadModule(dir + "\\" + Path.GetFileName(dir) + ".dll", config);
			}
			catch (Exception ex)
			{
				Far.Api.ShowError("Error on loading " + dir, ex);
			}
		}

		// write and free the cache
		_Cache.Update();
		_Cache = null;
		foreach (var manager in _Managers.Values)
			manager.DropCache();

		// free config data
		Config.Default.Reset();
	}

	// Loads the module assembly.
	void LoadModule(string assemblyPath, Config.Data config)
	{
		// use the file info to reduce file access
		var assemblyFileInfo = new FileInfo(assemblyPath);

		// try load from the cache
		if (LoadModuleFromCache(assemblyFileInfo, config))
			return;

		// add new module manager now, it will be removed on errors
		var manager = new ModuleManager(assemblyFileInfo.FullName);
		_Managers.Add(manager.ModuleName, manager);

		// load using reflection
		try
		{
			var module = config.GetModule(manager.ModuleName);
			manager.LoadConfig(module);

			var assembly = manager.LoadAssembly();
			foreach (var type in assembly.GetExportedTypes())
			{
				if (type.IsAbstract)
					continue;

				if (typeof(BaseModuleItem).IsAssignableFrom(type))
				{
					LoadModuleItemType(manager, type, module);
					continue;
				}

				if (typeof(ModuleSettingsBase).IsAssignableFrom(type))
				{
					var browsable = type.GetCustomAttribute<BrowsableAttribute>();
					if (browsable is null || browsable.Browsable)
						manager.AddSettingsTypeName(type.FullName);
					continue;
				}
			}

			// load a loadable host, if none or not loadable then cache
			if (!manager.LoadLoadableModuleHost())
				CacheModule(assemblyFileInfo, manager);
		}
		catch
		{
			RemoveModuleManager(manager);
			throw;
		}
	}

	// Loads the module from cache.
	// True if the module has been loaded from the cache.
	bool LoadModuleFromCache(FileInfo assemblyFileInfo, Config.Data config)
	{
		var assemblyPath = assemblyFileInfo.FullName;
		var manager = _Cache.Find(assemblyPath);
		if (manager is null)
			return false;

		// module info exists in cache
		++_Cache.CountFound;

		// most frequent reason to drop cache is different time
		if (manager.Timestamp != assemblyFileInfo.LastWriteTime.Ticks)
		{
			_Cache.Remove(assemblyPath);
			return false;
		}

		// module is legit for loading from cache
		try
		{
			// load config
			var module = config.GetModule(manager.ModuleName);
			manager.LoadConfig(module);

			// if the culture changed, drop cache
			var currentCultureName = manager.CurrentUICultureName();
			if (currentCultureName != manager.CachedUICulture)
				return false;

			// actions
			foreach (var action in manager.ProxyActions)
			{
				// register
				switch (action.Kind)
				{
					case ModuleItemKind.Command:
						{
							var it = (ProxyCommand)action;
							it.LoadConfig(module);
							Host.Instance.RegisterProxyCommand(it);
						}
						break;
					case ModuleItemKind.Drawer:
						{
							var it = (ProxyDrawer)action;
							it.LoadConfig(module);
							Host.Instance.RegisterProxyDrawer(it);
						}
						break;
					case ModuleItemKind.Editor:
						{
							var it = (ProxyEditor)action;
							it.LoadConfig(module);
							Host.Instance.RegisterProxyEditor(it);
						}
						break;
					case ModuleItemKind.Tool:
						{
							var it = (ProxyTool)action;
							it.LoadConfig(module);
							Host.Instance.RegisterProxyTool(it);
						}
						break;
					default:
						throw new ModuleException();
				}
			}

			// now module is loaded from cache, register
			_Managers.Add(manager.ModuleName, manager);
			return true;
		}
		catch (Exception ex)
		{
			// remove from cache
			_Cache.Remove(assemblyPath);

			// remove the manager
			RemoveModuleManager(manager);

			// ignore known
			if (ex is not ModuleException)
				throw new ModuleException($"Cannot read cache of '{assemblyPath}'.", ex);

			return false;
		}
	}

	// Loads one of <see cref="BaseModuleItem"/> types.
	static void LoadModuleItemType(ModuleManager manager, Type type, Config.Module config)
	{
		// command
		ProxyAction action;
		if (typeof(ModuleCommand).IsAssignableFrom(type))
		{
			var it = new ProxyCommand(manager, type);
			it.LoadConfig(config);
			Host.Instance.RegisterProxyCommand(it);
			action = it;
		}
		// drawer
		else if (typeof(ModuleDrawer).IsAssignableFrom(type))
		{
			var it = new ProxyDrawer(manager, type);
			it.LoadConfig(config);
			Host.Instance.RegisterProxyDrawer(it);
			action = it;
		}
		// editor
		else if (typeof(ModuleEditor).IsAssignableFrom(type))
		{
			var it = new ProxyEditor(manager, type);
			it.LoadConfig(config);
			Host.Instance.RegisterProxyEditor(it);
			action = it;
		}
		// tool
		else if (typeof(ModuleTool).IsAssignableFrom(type))
		{
			var it = new ProxyTool(manager, type);
			it.LoadConfig(config);
			Host.Instance.RegisterProxyTool(it);
			action = it;
		}
		// host
		else if (typeof(ModuleHost).IsAssignableFrom(type))
		{
			manager.SetModuleHostType(type);
			return;
		}
		else
		{
			throw new ModuleException("Unknown module item type.");
		}

		// to cache
		manager.ProxyActions.Add(action);
	}

	void CacheModule(FileInfo assemblyFileInfo, ModuleManager manager)
	{
		manager.Timestamp = assemblyFileInfo.LastWriteTime.Ticks;
		_Cache.Set(manager.AssemblyPath, manager);
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

	public static List<IModuleManager> GatherModuleManagers()
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
		foreach (IModuleAction action in Host.Actions.Values)
			if (ReferenceEquals(action.Manager, manager))
				actions.Add((ProxyAction)action);

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

	public static IModuleManager GetModuleManager(string name)
	{
		if (_Managers.TryGetValue(name, out ModuleManager manager))
			return manager;

		throw new ArgumentException("Cannot find the module name.");
	}
}
