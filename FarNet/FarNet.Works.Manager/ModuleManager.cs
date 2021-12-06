
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;

namespace FarNet.Works
{
	public sealed partial class ModuleManager : IModuleManager
	{
		const int idUICulture = 0;
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

		string GetConfigPath(bool create)
		{
			return GetFolderPath(SpecialFolder.RoamingData, create) + @"\FarNet.binary";
		}

		internal Hashtable ReadConfig()
		{
			var file = GetConfigPath(false);
			if (!File.Exists(file))
				return new Hashtable();

			object deserialized;
			var formatter = new BinaryFormatter();
			using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
				deserialized = formatter.Deserialize(stream);

			return deserialized as Hashtable ?? new Hashtable();
		}

		internal void LoadConfig(Hashtable data)
		{
			_StoredUICulture = data[idUICulture] as string;
		}

		internal void SaveConfig(Hashtable data)
		{
			if (string.IsNullOrEmpty(_StoredUICulture))
				data.Remove(idUICulture);
			else
				data[idUICulture] = _StoredUICulture;
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
			it.LoadConfig((Hashtable)ReadConfig()[it.Id]);

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
			it.LoadConfig((Hashtable)ReadConfig()[it.Id]);

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
			it.LoadConfig((Hashtable)ReadConfig()[it.Id]);

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

		// faster than CurrentUICulture.Name
		internal string CurrentUICultureName()
		{
			if (_CurrentUICulture is not null)
				return _CurrentUICulture.Name;

			if (!string.IsNullOrEmpty(_StoredUICulture))
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
					// load, try, drop bad, keep mom
					string cultureName = StoredUICulture;
					if (cultureName.Length > 0)
					{
						try
						{
							_CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
						}
						catch (ArgumentException)
						{
							_StoredUICulture = null;
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

		string _StoredUICulture;
		public override string StoredUICulture
		{
			get => _StoredUICulture ?? string.Empty;
			set => _StoredUICulture = value;
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

		// * This methods can be "slow", it is called from UI only.
		// * Read data first and merge with current.
		// We have actions registered explicitly, they may be not loaded now and still have data to preserve.
		public override void SaveConfig()
		{
			// read existing
			var config = ReadConfig();

			// save module data
			SaveConfig(config);

			// save action data
			foreach (ProxyAction action in Host.Actions.Values)
			{
				if (action.Manager != this)
					continue;

				var data = action.SaveConfig();
				if (data is null || data.Count == 0)
					config.Remove(action.Id);
				else
					config[action.Id] = data;
			}

			// write merged
			var formatter = new BinaryFormatter();
			using var stream = new FileStream(GetConfigPath(true), FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, config);
		}
	}
}
