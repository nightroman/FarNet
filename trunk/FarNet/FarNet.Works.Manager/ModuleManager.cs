
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;

namespace FarNet.Works
{
	public sealed class ModuleManager : IModuleManager
	{
		// Assembly
		string _AssemblyPath;
		Assembly _AssemblyInstance;
		CultureInfo _CurrentUICulture;
		ResourceManager _ResourceManager;

		// Module host
		ModuleHost _ModuleHostInstance;
		string _ModuleHostClassName;
		Type _ModuleHostClassType;

		internal ModuleManager(string assemblyPath)
		{
			_AssemblyPath = assemblyPath;
		}

		void Connect()
		{
			_ModuleHostInstance = (ModuleHost)CreateEntry(_ModuleHostClassType);
			_ModuleHostClassType = null;

			Log.Source.TraceInformation("Connect {0}", _ModuleHostInstance);
			_ModuleHostInstance.Connect();
		}

		internal BaseModuleItem CreateEntry(Type type)
		{
			// create the instance
			BaseModuleItem instance = (BaseModuleItem)Activator.CreateInstance(type);

			// connect the instance
			instance.Manager = this;

			return instance;
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
				_ModuleHostClassType = AssemblyInstance.GetType(_ModuleHostClassName, true, false);
				_ModuleHostClassName = null;
			}

			if (_ModuleHostClassType != null)
				Connect();

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

			Connect();
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
		internal Assembly AssemblyInstance
		{
			get
			{
				if (_AssemblyInstance == null)
					_AssemblyInstance = Assembly.LoadFrom(_AssemblyPath);

				return _AssemblyInstance;
			}
		}

		internal string AssemblyPath
		{
			get
			{
				return _AssemblyPath;
			}
		}

		internal bool CachedResources { get; set; }

		public string ModuleName
		{
			get
			{
				return Path.GetFileName(_AssemblyPath);
			}
		}

		public CultureInfo CurrentUICulture
		{
			get
			{
				// once
				if (_CurrentUICulture == null)
				{
					// load, try, drop bad, keep mom
					string cultureName = Host.Instance.LoadFarNetValue(ModuleName, "UICulture", string.Empty).ToString();
					if (cultureName.Length > 0)
					{
						try
						{
							_CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
						}
						catch (ArgumentException)
						{
							Host.Instance.SaveFarNetValue(ModuleName, "UICulture", null);
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

		public string StoredUICulture
		{
			get
			{
				return Host.Instance.LoadFarNetValue(ModuleName, "UICulture", string.Empty).ToString();
			}
			set
			{
				Host.Instance.SaveFarNetValue(ModuleName, "UICulture", value);
			}
		}

		public IRegistryKey OpenRegistryKey(string name, bool writable)
		{
			return Host.Instance.OpenModuleKey(ModuleName + "\\" + name, writable);
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

#if false
public: // IModuleManager
		public void Unregister()
		{
			Host.Instance.Unregister();
		}

	virtual IModuleCommand^ RegisterModuleCommand(Guid id, ModuleCommandAttribute^ attribute, EventHandler<ModuleCommandEventArgs^>^ handler);
	virtual IModuleFiler^ RegisterModuleFiler(Guid id, ModuleFilerAttribute^ attribute, EventHandler<ModuleFilerEventArgs^>^ handler);
	virtual IModuleTool^ RegisterModuleTool(Guid id, ModuleToolAttribute^ attribute, EventHandler<ModuleToolEventArgs^>^ handler);
internal:
	ModuleManager(String^ assemblyPath);
	BaseModuleItem^ CreateEntry(Type^ type);
	bool HasHost() { return _ModuleHostInstance || _ModuleHostClassName || _ModuleHostClassType; }
	ModuleHost^ GetLoadedModuleHost() { return _ModuleHostInstance; }
	property Assembly^ AssemblyInstance { Assembly^ get(); }
	property bool CachedResources;
	property String^ AssemblyPath { String^ get(); }
	String^ GetModuleHostClassName();
	void Invoking();
	void SetModuleHost(String^ moduleHostClassName);
	void SetModuleHost(Type^ moduleHostClassType);
	bool LoadLoadableModuleHost();
internal:
	static Object^ LoadFarNetValue(String^ keyPath, String^ valueName, Object^ defaultValue);
	static void SaveFarNetValue(String^ keyPath, String^ valueName, Object^ value);
private:
	void Connect();
#endif
	}
}
