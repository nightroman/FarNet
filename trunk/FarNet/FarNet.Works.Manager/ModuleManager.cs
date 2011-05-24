
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2011 FarNet Team
*/

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;

namespace FarNet.Works
{
	public sealed class ModuleManager : IModuleManager
	{
		const int idUICulture = 0;
		// Assembly
		string _AssemblyPath;
		Assembly _AssemblyInstance;
		CultureInfo _CurrentUICulture;
		ResourceManager _ResourceManager;
		internal bool HasSettings { get; set; }
		// Module host
		ModuleHost _ModuleHostInstance;
		string _ModuleHostClassName;
		Type _ModuleHostClassType;
		// Module settings
		internal ModuleManager(string assemblyPath)
		{
			_AssemblyPath = assemblyPath;
		}
		string GetSettingsFileName()
		{
			return GetFolderPath(SpecialFolder.RoamingData) + @"\FarNet.binary";
		}
		internal Hashtable ReadSettings()
		{
			var file = GetSettingsFileName();
			if (!File.Exists(file))
				return new Hashtable();

			object deserialized;
			var formatter = new BinaryFormatter();
			using (var stream = new FileStream(GetSettingsFileName(), FileMode.Open, FileAccess.Read, FileShare.Read))
				deserialized = formatter.Deserialize(stream);

			return deserialized as Hashtable ?? new Hashtable();
		}
		internal void LoadData(Hashtable data)
		{
			_StoredUICulture = data[idUICulture] as string;
		}
		internal void SaveData(Hashtable data)
		{
			if (string.IsNullOrEmpty(_StoredUICulture))
				data.Remove(idUICulture);
			else
				data[idUICulture] = _StoredUICulture;
		}
		void ConnectModuleHost()
		{
			_ModuleHostInstance = (ModuleHost)CreateEntry(_ModuleHostClassType);
			_ModuleHostClassType = null;

			Log.Source.TraceInformation("Connect {0}", _ModuleHostInstance);
			_ModuleHostInstance.Connect();
		}
		internal BaseModuleItem CreateEntry(Type type)
		{
			return (BaseModuleItem)Activator.CreateInstance(type);
		}
		internal ModuleHost GetLoadedModuleHost()
		{
			return _ModuleHostInstance;
		}
		internal string GetModuleHostClassName()
		{
			if (_ModuleHostClassName != null)
				return _ModuleHostClassName;

			if (_ModuleHostClassType != null)
				return _ModuleHostClassType.FullName;

			if (_ModuleHostInstance != null)
				return _ModuleHostInstance.GetType().FullName;

			return null;
		}
		internal bool HasHost()
		{
			return _ModuleHostInstance != null || _ModuleHostClassName != null || _ModuleHostClassType != null;
		}
		internal void Invoking()
		{
			if (_ModuleHostClassName != null)
			{
				_ModuleHostClassType = LoadAssembly().GetType(_ModuleHostClassName, true, false);
				_ModuleHostClassName = null;
			}

			if (_ModuleHostClassType != null)
				ConnectModuleHost();

			if (_ModuleHostInstance != null)
				_ModuleHostInstance.Invoking();
		}
		internal bool LoadLoadableModuleHost()
		{
			if (_ModuleHostClassType == null)
				return false;

			object[] attrs = _ModuleHostClassType.GetCustomAttributes(typeof(ModuleHostAttribute), false);
			if (attrs.Length == 0 || !((ModuleHostAttribute)attrs[0]).Load)
				return false;

			ConnectModuleHost();
			return true;
		}
		public IModuleCommand RegisterModuleCommand(Guid id, ModuleCommandAttribute attribute, EventHandler<ModuleCommandEventArgs> handler)
		{
			if (handler == null)
				throw new ArgumentNullException("handler");
			if (attribute == null)
				throw new ArgumentNullException("attribute");
			if (string.IsNullOrEmpty(attribute.Name))
				throw new ArgumentException("'attribute.Name' must not be empty.");

			ProxyCommand it = new ProxyCommand(this, id, attribute, handler);
			it.LoadData((Hashtable)ReadSettings()[it.Id]);

			Host.Instance.RegisterProxyCommand(it);
			return it;
		}
		public IModuleFiler RegisterModuleFiler(Guid id, ModuleFilerAttribute attribute, EventHandler<ModuleFilerEventArgs> handler)
		{
			if (handler == null)
				throw new ArgumentNullException("handler");
			if (attribute == null)
				throw new ArgumentNullException("attribute");
			if (string.IsNullOrEmpty(attribute.Name))
				throw new ArgumentException("'attribute.Name' must not be empty.");

			ProxyFiler it = new ProxyFiler(this, id, attribute, handler);
			it.LoadData((Hashtable)ReadSettings()[it.Id]);

			Host.Instance.RegisterProxyFiler(it);
			return it;
		}
		public IModuleTool RegisterModuleTool(Guid id, ModuleToolAttribute attribute, EventHandler<ModuleToolEventArgs> handler)
		{
			if (handler == null)
				throw new ArgumentNullException("handler");
			if (attribute == null)
				throw new ArgumentNullException("attribute");
			if (string.IsNullOrEmpty(attribute.Name))
				throw new ArgumentException("'attribute.Name' must not be empty.");

			ProxyTool it = new ProxyTool(this, id, attribute, handler);
			it.LoadData((Hashtable)ReadSettings()[it.Id]);

			Host.Instance.RegisterProxyTool(it);
			return it;
		}
		internal void SetModuleHost(string moduleHostClassName)
		{
			if (HasHost())
				throw new ModuleException("The module host is already set.");

			_ModuleHostClassName = moduleHostClassName;
		}
		internal void SetModuleHost(Type moduleHostClassType)
		{
			if (HasHost())
				throw new ModuleException("The module host is already set.");

			_ModuleHostClassType = moduleHostClassType;
		}
		//! Don't use Far UI
		[
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"),
		]
		public void Unregister()
		{
			Log.Source.TraceInformation("Unregister module {0}", ModuleName);

			if (_ModuleHostInstance == null)
			{
				ModuleLoader.RemoveModuleManager(this);
				return;
			}

			try
			{
				Log.Source.TraceInformation("Disconnect {0}", _ModuleHostInstance);
				_ModuleHostInstance.Disconnect();
			}
			catch (Exception ex)
			{
				Far.Net.ShowError("ERROR: module " + _ModuleHostInstance, ex);
			}
			finally
			{
				_ModuleHostInstance = null;

				ModuleLoader.RemoveModuleManager(this);
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
		public Assembly LoadAssembly()
		{
			if (_AssemblyInstance == null)
				_AssemblyInstance = Assembly.LoadFrom(_AssemblyPath);

			return _AssemblyInstance;
		}
		internal string AssemblyPath
		{
			get { return _AssemblyPath; }
		}
		internal bool CachedResources { get; set; }
		public string ModuleName
		{
			get { return Path.GetFileNameWithoutExtension(_AssemblyPath); }
		}
		public CultureInfo CurrentUICulture
		{
			get
			{
				// once
				if (_CurrentUICulture == null)
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
					if (_CurrentUICulture == null)
						_CurrentUICulture = Far.Net.GetCurrentUICulture(false);
				}

				return _CurrentUICulture;
			}
			set
			{
				_CurrentUICulture = value;
			}
		}
		string _StoredUICulture;
		public string StoredUICulture
		{
			get { return _StoredUICulture ?? string.Empty; }
			set { _StoredUICulture = value; }
		}
		public string GetString(string name)
		{
			if (_ResourceManager == null)
			{
				string baseName = Path.GetFileNameWithoutExtension(_AssemblyPath);
				string resourceDir = Path.GetDirectoryName(_AssemblyPath);
				_ResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName, resourceDir, null);
			}

			return _ResourceManager.GetString(name, CurrentUICulture);
		}
		public string GetFolderPath(SpecialFolder folder)
		{
			var dir = Far.Net.GetFolderPath(folder) + @"\FarNet\" + ModuleName;
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			return dir;
		}
		// NB: It is fine to be slow, called from UI.
		public void SaveSettings()
		{
			// read from disk
			var settings = ReadSettings();

			// save module data
			SaveData(settings);

			// save action data
			foreach (ProxyAction action in Host.Actions.Values)
			{
				if (action.Manager != this)
					continue;

				var data = action.SaveData();
				if (data == null || data.Count == 0)
					settings.Remove(action.Id);
				else
					settings[action.Id] = data;
			}

			var formatter = new BinaryFormatter();
			using (var stream = new FileStream(GetSettingsFileName(), FileMode.Create, FileAccess.Write, FileShare.None))
				formatter.Serialize(stream, settings);
		}
	}
}
