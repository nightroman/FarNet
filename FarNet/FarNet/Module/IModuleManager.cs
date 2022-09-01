
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace FarNet;

/// <summary>
/// The module manager shared between all module items.
/// </summary>
public abstract class IModuleManager
{
	/// <summary>
	/// Gets or sets the current UI culture.
	/// </summary>
	/// <remarks>
	/// Method <see cref="GetString"/> gets localized strings depending exactly on this property value.
	/// This property is set internally to the Far UI culture and normally you do not care of it.
	/// But you may want to set it yourself:
	/// <ul>
	/// <li>To ensure it is the same as the current Far UI culture after its changes.</li>
	/// <li>To use cultures different from the current Far UI culture (for testing or whatever).</li>
	/// <li>To use cultures which are not even known to Far itself (there are no such .lng files).</li>
	/// </ul>
	/// </remarks>
	public abstract CultureInfo CurrentUICulture { get; set; }

	/// <summary>
	/// The <see cref="BaseModuleItem.GetString"/> worker.
	/// </summary>
	/// <param name="name">The string name.</param>
	public abstract string GetString(string name);

	/// <summary>
	/// Gets the path to the system special folder that is identified by the specified enumeration.
	/// </summary>
	/// <param name="folder">Special folder enumeration.</param>
	/// <param name="create">Tells to create the directory if it does not exist.</param>
	/// <remarks>
	/// <para>
	/// Local and roaming data directories are designed for module data and settings files.
	/// NOTE: Names like <b>FarNet.*</b> are reserved for the internal use.
	/// </para>
	/// <para>
	/// If <c>create</c> is true and the directory does not exist and cannot be created in
	/// the usual location (e.g. it is read only) then it is created in <c>%TEMP%\FarNet</c>.
	/// </para>
	/// </remarks>
	public abstract string GetFolderPath(SpecialFolder folder, bool create);

	/// <summary>
	/// Unregisters the module in critical cases.
	/// </summary>
	/// <remarks>
	/// This method should be called only on fatal errors and similar cases.
	/// </remarks>
	public abstract void Unregister();

	/// <summary>
	/// OBSOLETE
	/// </summary>
	/// <param name="id">.</param>
	/// <param name="handler">.</param>
	/// <param name="attribute">.</param>
	[Obsolete("Use RegisterCommand")]
	public IModuleCommand RegisterModuleCommand(Guid id, ModuleCommandAttribute attribute, EventHandler<ModuleCommandEventArgs> handler)
	{
		attribute.Id = id.ToString();
		return RegisterCommand(attribute, handler);
	}

	/// <summary>
	/// Registers the command handler invoked from the command line by its prefix.
	/// </summary>
	/// <param name="handler">Command handler.</param>
	/// <param name="attribute">Command attribute.</param>
	/// <remarks>
	/// NOTE: Consider to implement the <see cref="ModuleCommand"/> instead.
	/// Dynamic registration is not recommended for standard scenarios.
	/// <include file='doc.xml' path='doc/RegisterModule/*'/>
	/// </remarks>
	public abstract IModuleCommand RegisterCommand(ModuleCommandAttribute attribute, EventHandler<ModuleCommandEventArgs> handler);

	/// <summary>
	/// OBSOLETE
	/// </summary>
	/// <param name="id">.</param>
	/// <param name="handler">.</param>
	/// <param name="attribute">.</param>
	[Obsolete("Use RegisterDrawer")]
	public IModuleDrawer RegisterModuleDrawer(Guid id, ModuleDrawerAttribute attribute, Action<IEditor, ModuleDrawerEventArgs> handler)
	{
		attribute.Id = id.ToString();
		return RegisterDrawer(attribute, handler);
	}

	/// <summary>
	/// Registers the editor drawer handler.
	/// </summary>
	/// <param name="handler">Drawer handler.</param>
	/// <param name="attribute">Drawer attribute.</param>
	/// <remarks>
	/// NOTE: Consider to implement the <see cref="ModuleDrawer"/> instead.
	/// Dynamic registration is not recommended for standard scenarios.
	/// <include file='doc.xml' path='doc/RegisterModule/*'/>
	/// </remarks>
	public abstract IModuleDrawer RegisterDrawer(ModuleDrawerAttribute attribute, Action<IEditor, ModuleDrawerEventArgs> handler);

	/// <summary>
	/// OBSOLETE
	/// </summary>
	/// <param name="id">.</param>
	/// <param name="handler">.</param>
	/// <param name="attribute">.</param>
	[Obsolete("Use RegisterTool")]
	public IModuleTool RegisterModuleTool(Guid id, ModuleToolAttribute attribute, EventHandler<ModuleToolEventArgs> handler)
	{
		attribute.Id = id.ToString();
		return RegisterTool(attribute, handler);
	}

	/// <summary>
	/// Registers the tool handler invoked from one of Far menus.
	/// </summary>
	/// <param name="handler">Tool handler.</param>
	/// <param name="attribute">Tool attribute.</param>
	/// <remarks>
	/// NOTE: Consider to implement the <see cref="ModuleTool"/> instead.
	/// Dynamic registration is not recommended for standard scenarios.
	/// <include file='doc.xml' path='doc/RegisterModule/*'/>
	/// </remarks>
	public abstract IModuleTool RegisterTool(ModuleToolAttribute attribute, EventHandler<ModuleToolEventArgs> handler);

	/// <summary>
	/// Gets the module name.
	/// </summary>
	public abstract string ModuleName { get; }

	/// <summary>
	/// INTERNAL
	/// </summary>
	public abstract string StoredUICulture { get; set; }

	/// <summary>
	/// INTERNAL
	/// </summary>
	/// <param name="connect">INTERNAL</param>
	public abstract Assembly LoadAssembly(bool connect);

	/// <summary>
	/// INTERNAL
	/// </summary>
	public abstract void SaveConfig();

	/// <summary>
	/// INTERNAL
	/// </summary>
	public abstract IReadOnlyList<string> SettingsTypeNames { get; }

	/// <summary>
	/// Calls <see cref="ModuleHost.Interop"/>.
	/// </summary>
	/// <param name="command">.</param>
	/// <param name="args">.</param>
	/// <returns>.</returns>
	public abstract object Interop(string command, object args);
}
