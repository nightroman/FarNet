
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;

namespace FarNet.Works
{
	public sealed partial class ModuleManager : IModuleManager
	{
		Assembly _AssemblyInstance;
		CultureInfo _CurrentUICulture;
		ResourceManager _ResourceManager;
		internal string AssemblyPath { get; }

		// Module host
		string _ModuleHostTypeName;
		Type _ModuleHostType;
		ModuleHost _ModuleHost;

		// from cache or reflection
		readonly List<string> _SettingsTypeNames = new();
		// used by loader on reflection
		public void AddSettingsTypeName(string name) => _SettingsTypeNames.Add(name);
		// used for the settings menu and caching
		public override IReadOnlyList<string> SettingsTypeNames => _SettingsTypeNames;

		// New module manager
		internal ModuleManager(string assemblyPath)
		{
			AssemblyPath = assemblyPath;
		}

		/// <summary>
		/// Sets properties from data, if not null.
		/// </summary>
		internal void LoadConfig(Config.Module config)
		{
			if (config is not null)
				_StoredUICulture = config.Culture;
		}

		internal void SaveConfig(Config.Module config)
		{
			config.Culture = _StoredUICulture;
		}

		void ConnectModuleHost()
		{
			_ModuleHost = (ModuleHost)Activator.CreateInstance(_ModuleHostType, false);
			_ModuleHostType = null;
			_ModuleHost.Connect();
		}

		internal ModuleHost GetLoadedModuleHost()
		{
			return _ModuleHost;
		}

		internal string GetModuleHostClassName()
		{
			if (_ModuleHostTypeName is not null)
				return _ModuleHostTypeName;

			if (_ModuleHostType is not null)
				return _ModuleHostType.FullName;

			if (_ModuleHost is not null)
				return _ModuleHost.GetType().FullName;

			return null;
		}

		[Conditional("DEBUG")]
		internal void AssertNoHost()
		{
			Debug.Assert(_ModuleHost is null && _ModuleHostTypeName is null && _ModuleHostType is null);
		}

		internal void Invoking()
		{
			if (_ModuleHostTypeName is not null)
			{
				_ModuleHostType = LoadAssembly().GetType(_ModuleHostTypeName, true, false);
				_ModuleHostTypeName = null;
			}

			if (_ModuleHostType is not null)
				ConnectModuleHost();

			if (_ModuleHost is not null)
				_ModuleHost.Invoking();
		}

		public override object Interop(string command, object args)
		{
			Invoking();

			if (_ModuleHost is null)
				throw new InvalidOperationException("Module does not have a host.");

			return _ModuleHost.Interop(command, args);
		}

		internal bool LoadLoadableModuleHost()
		{
			if (_ModuleHostType is null)
				return false;

			object[] attrs = _ModuleHostType.GetCustomAttributes(typeof(ModuleHostAttribute), false);
			if (attrs.Length == 0 || !((ModuleHostAttribute)attrs[0]).Load)
				return false;

			ConnectModuleHost();
			return true;
		}

		public override IModuleCommand RegisterModuleCommand(Guid id, ModuleCommandAttribute attribute, EventHandler<ModuleCommandEventArgs> handler)
		{
			if (handler is null)
				throw new ArgumentNullException("handler");
			if (attribute is null)
				throw new ArgumentNullException("attribute");
			if (string.IsNullOrEmpty(attribute.Name))
				throw new ArgumentException("'attribute.Name' must not be empty.");

			var it = new ProxyCommand(this, id, attribute, handler);
			var config = Config.Default.GetData();
			it.LoadConfig(config.GetModule(ModuleName));

			Host.Instance.RegisterProxyCommand(it);
			return it;
		}

		public override IModuleDrawer RegisterModuleDrawer(Guid id, ModuleDrawerAttribute attribute, Action<IEditor, ModuleDrawerEventArgs> handler)
		{
			if (handler is null)
				throw new ArgumentNullException("handler");
			if (attribute is null)
				throw new ArgumentNullException("attribute");
			if (string.IsNullOrEmpty(attribute.Name))
				throw new ArgumentException("'attribute.Name' must not be empty.");

			var it = new ProxyDrawer(this, id, attribute, handler);
			var config = Config.Default.GetData();
			it.LoadConfig(config.GetModule(ModuleName));

			Host.Instance.RegisterProxyDrawer(it);
			return it;
		}

		public override IModuleTool RegisterModuleTool(Guid id, ModuleToolAttribute attribute, EventHandler<ModuleToolEventArgs> handler)
		{
			if (handler is null)
				throw new ArgumentNullException("handler");
			if (attribute is null)
				throw new ArgumentNullException("attribute");
			if (string.IsNullOrEmpty(attribute.Name))
				throw new ArgumentException("'attribute.Name' must not be empty.");

			var it = new ProxyTool(this, id, attribute, handler);
			var config = Config.Default.GetData();
			it.LoadConfig(config.GetModule(ModuleName));

			Host.Instance.RegisterProxyTool(it);
			return it;
		}

		internal void SetModuleHostType(Type type)
		{
			AssertNoHost();

			_ModuleHostType = type;
		}

		internal void SetModuleHostTypeName(string type)
		{
			AssertNoHost();

			_ModuleHostTypeName = type;
		}

		//! Don't use Far UI
		public override void Unregister()
		{
			if (_ModuleHost is null)
			{
				ModuleLoader.RemoveModuleManager(this);
				return;
			}

			try
			{
				_ModuleHost.Disconnect();
			}
			catch (Exception ex)
			{
				Far.Api.ShowError("ERROR: module " + _ModuleHost, ex);
			}
			finally
			{
				_ModuleHost = null;

				ModuleLoader.RemoveModuleManager(this);
			}
		}

		public Assembly LoadAssembly()
		{
			if (_AssemblyInstance is null)
				_AssemblyInstance = Assembly.LoadFrom(AssemblyPath);

			return _AssemblyInstance;
		}

		public override Assembly LoadAssembly(bool connect)
		{
			if (connect)
				Invoking();
			else
				LoadAssembly();

			return _AssemblyInstance;
		}

		public override string ModuleName
		{
			get { return Path.GetFileNameWithoutExtension(AssemblyPath); }
		}

		// faster than `CurrentUICulture.Name`
		internal string CurrentUICultureName()
		{
			if (_CurrentUICulture is not null)
				return _CurrentUICulture.Name;

			if (_StoredUICulture is not null)
				return _StoredUICulture;

			_CurrentUICulture = Far.Api.GetCurrentUICulture(false);
			return _CurrentUICulture.Name;
		}

		public override CultureInfo CurrentUICulture
		{
			get
			{
				// once
				if (_CurrentUICulture is null)
				{
					// try, drop bad, keep mom
					if (_StoredUICulture is not null)
					{
						try
						{
							_CurrentUICulture = CultureInfo.GetCultureInfo(_StoredUICulture);
						}
						catch (ArgumentException)
						{
							// drop bad culture
							_StoredUICulture = null;

							// drop in config, too (do not Reset(), config may be in use)
							var config = Config.Default.GetData();
							var module = config.GetModule(ModuleName);
							if (module is not null)
							{
								module.Culture = null;
								if (module.IsDefault())
									config.RemoveModule(ModuleName);
								Config.Default.Save();
							}
						}
					}

					// not yet? use current
					if (_CurrentUICulture is null)
						_CurrentUICulture = Far.Api.GetCurrentUICulture(false);
				}
				return _CurrentUICulture;
			}
			set
			{
				_CurrentUICulture = value;
			}
		}

		/// <summary>
		/// Valur from config (null ~ default).
		/// </summary>
		string _StoredUICulture;

		/// <summary>
		/// Wraps internal value: get: null to empty; set: empty to null.
		/// </summary>
		public override string StoredUICulture
		{
			get => _StoredUICulture ?? string.Empty;
			set => _StoredUICulture = string.IsNullOrEmpty(value) ? null : value;
		}

		public override string GetString(string name)
		{
			if (_ResourceManager is null)
			{
				string baseName = Path.GetFileNameWithoutExtension(AssemblyPath);
				string resourceDir = Path.GetDirectoryName(AssemblyPath);
				_ResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName, resourceDir, null);
			}

			return _ResourceManager.GetString(name, CurrentUICulture);
		}

		public override string GetFolderPath(SpecialFolder folder, bool create)
		{
			// normal dir
			var dir = Path.Combine(Far.Api.GetFolderPath(folder), @"FarNet\" + ModuleName);
			if (create && !Directory.Exists(dir))
			{
				try
				{
					Directory.CreateDirectory(dir);
				}
				catch (IOException)
				{
					// temp dir
					dir = Path.Combine(Path.GetTempPath(), @"FarNet\" + folder.ToString() + @"\" + ModuleName);
					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);
				}
			}
			return dir;
		}

		// This methods is "slow", UI only.
		// Merge existing module data with current.
		// We have actions added manually, not loaded but with data to keep.
		public override void SaveConfig()
		{
			// get data with reset for the latest
			Config.Default.Reset();
			var config = Config.Default.GetData();

			// get existing and save module data
			var module = config.GetModule(ModuleName) ?? new();
			SaveConfig(module);

			// save action data
			foreach (var action in Host.Actions.Values)
			{
				if (action.Manager != this)
					continue;

				switch (action.Kind)
				{
					case ModuleItemKind.Command:
						{
							var it1 = (ProxyCommand)action;
							var it2 = it1.SaveConfig();
							module.SetCommand(it1.Id, it2);
						}
						break;
					case ModuleItemKind.Drawer:
						{
							var it1 = (ProxyDrawer)action;
							var it2 = it1.SaveConfig();
							module.SetDrawer(it1.Id, it2);
						}
						break;
					case ModuleItemKind.Editor:
						{
							var it1 = (ProxyEditor)action;
							var it2 = it1.SaveConfig();
							module.SetEditor(it1.Id, it2);
						}
						break;
					case ModuleItemKind.Tool:
						{
							var it1 = (ProxyTool)action;
							var it2 = it1.SaveConfig();
							module.SetTool(it1.Id, it2);
						}
						break;
				}
			}

			// remove default or set module
			if (module.IsDefault())
			{
				config.RemoveModule(ModuleName);
			}
			else
			{
				module.Name = ModuleName;
				config.SetModule(module);
			}

			// save merged
			Config.Default.Save();
			Config.Default.Reset();
		}
	}
}
