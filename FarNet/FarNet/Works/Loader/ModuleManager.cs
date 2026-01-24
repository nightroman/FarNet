using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace FarNet.Works;

sealed partial class ModuleManager : IModuleManager
{
	Assembly? _AssemblyInstance;
	CultureInfo? _CurrentUICulture;
	ResourceManager? _ResourceManager;
	internal string AssemblyPath { get; }

	// Module host
	string? _hostTypeName;
	ModuleHost? _host;

	internal ModuleManager(string assemblyPath)
	{
		AssemblyPath = assemblyPath;
	}

	// Sets properties from data, if not null.
	internal void LoadConfig(Config.Module? config)
	{
		if (config is not null)
			_StoredUICulture = config.Culture;
	}

	internal void SaveConfig(Config.Module config)
	{
		config.Culture = _StoredUICulture;
	}

	// cached case, just keep the name
	internal void SetHostTypeName(string typeName)
	{
		Debug.Assert(_host is null && _hostTypeName is null);

		_hostTypeName = typeName;
	}

	// loaded case, create the instance
	internal void SetHostType(Type type)
	{
		Debug.Assert(_host is null && _hostTypeName is null);

		_host = (ModuleHost)Activator.CreateInstance(type, false)!;
		ToUseEditors = _host.ToUseEditors;
	}

		void EnsureHost()
	{
		if (_host is null)
		{
			var type = LoadAssembly().GetType(_hostTypeName!, true, false)!;
			_hostTypeName = null;
			SetHostType(type);
		}
	}

	internal ModuleHost GetHost()
	{
		if (_host is null)
			EnsureHost();

		return _host!;
	}

	internal string? GetHostTypeName()
	{
		if (_hostTypeName is { })
			return _hostTypeName;

		if (_host is { })
			return _host.GetType().FullName;

		return null;
	}

	internal void Invoking()
	{
		if (_hostTypeName is { })
			EnsureHost();

		_host?.Invoking();
	}

	internal bool ShouldCache()
	{
		Debug.Assert(_hostTypeName is null);

		// no host ~ cache
		if (_host is null)
			return true;

		if (_host.ToLoad)
			return false;

		return false;
	}

	public override object Interop(string command, object? args)
	{
		Invoking();

		if (_host is null)
			throw new InvalidOperationException("Module does not have a host.");

		return _host.Interop(command, args);
	}

	public override IModuleCommand RegisterCommand(ModuleCommandAttribute attribute, EventHandler<ModuleCommandEventArgs> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		ArgumentNullException.ThrowIfNull(attribute);
		if (string.IsNullOrEmpty(attribute.Name))
			throw new ArgumentException("'attribute.Name' must not be empty.");
		if (!Guid.TryParse(attribute.Id, out Guid id))
			throw new ArgumentException("'attribute.Id' has invalid GUID.");
		if (string.IsNullOrEmpty(attribute.Prefix))
			throw new ArgumentException("'attribute.Prefix' must not be empty.");

		var it = new ProxyCommand(this, id, attribute, handler);
		var config = Config.Default.GetData();
		it.LoadConfig(config.GetModule(ModuleName));

		Far2.Api.RegisterProxyCommand(it);
		return it;
	}

	public override IModuleDrawer RegisterDrawer(ModuleDrawerAttribute attribute, Action<IEditor, ModuleDrawerEventArgs> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		ArgumentNullException.ThrowIfNull(attribute);
		if (string.IsNullOrEmpty(attribute.Name))
			throw new ArgumentException("'attribute.Name' must not be empty.");
		if (!Guid.TryParse(attribute.Id, out Guid id))
			throw new ArgumentException("'attribute.Id' has invalid GUID.");

		var it = new ProxyDrawer(this, id, attribute, handler);
		var config = Config.Default.GetData();
		it.LoadConfig(config.GetModule(ModuleName));

		Far2.Api.RegisterProxyDrawer(it);
		return it;
	}

	public override IModuleTool RegisterTool(ModuleToolAttribute attribute, EventHandler<ModuleToolEventArgs> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		ArgumentNullException.ThrowIfNull(attribute);
		if (string.IsNullOrEmpty(attribute.Name))
			throw new ArgumentException("'attribute.Name' must not be empty.");
		if (!Guid.TryParse(attribute.Id, out Guid id))
			throw new ArgumentException("'attribute.Id' has invalid GUID.");

		var it = new ProxyTool(this, id, attribute, handler);
		var config = Config.Default.GetData();
		it.LoadConfig(config.GetModule(ModuleName));

		Far2.Api.RegisterProxyTool(it);
		return it;
	}

	//! Don't use Far UI
	public override void Unregister()
	{
		if (_host is null)
		{
			ModuleLoader.RemoveModuleManager(this);
			return;
		}

		try
		{
			if (_host is IDisposable disposable)
				disposable.Dispose();
		}
		catch (Exception ex)
		{
			Far.Api.ShowError("ERROR: module " + _host, ex);
		}
		finally
		{
			_host = null;

			ModuleLoader.RemoveModuleManager(this);
		}
	}

	public Assembly LoadAssembly()
	{
		if (_AssemblyInstance is not null)
			return _AssemblyInstance;

		var deps = Path.ChangeExtension(AssemblyPath, "deps.json");
		if (File.Exists(deps))
		{
			var loadContext = new AssemblyLoadContext2(AssemblyPath);
			_AssemblyInstance = loadContext.LoadFromAssemblyPath(AssemblyPath);
		}
		else
		{
			_AssemblyInstance = Assembly.LoadFrom(AssemblyPath);
		}

		return _AssemblyInstance;
	}

	public override Assembly LoadAssembly(bool connect)
	{
		if (connect)
			Invoking();
		else
			LoadAssembly();

		return _AssemblyInstance!;
	}

	public override string ModuleName => Path.GetFileNameWithoutExtension(AssemblyPath);

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
				_CurrentUICulture ??= Far.Api.GetCurrentUICulture(false);
			}
			return _CurrentUICulture;
		}
		set
		{
			_CurrentUICulture = value;
		}
	}

	// Value from config (null ~ default).
	string? _StoredUICulture;

	// Wraps internal value: get: null to empty; set: empty to null.
	public override string StoredUICulture
	{
		get => _StoredUICulture ?? string.Empty;
		set => _StoredUICulture = string.IsNullOrEmpty(value) ? null : value;
	}

	public override string? GetString(string name)
	{
		if (_ResourceManager is null)
		{
			string baseName = Path.GetFileNameWithoutExtension(AssemblyPath);
			string resourceDir = Path.GetDirectoryName(AssemblyPath)!;
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
		foreach (var action in Far2.Actions.Values)
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
