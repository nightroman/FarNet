
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Globalization;
using System.IO;

// _220609_gf Why help methods moved from IFar.
// - New methods do not use unreliable calling assemblies.
// - Old methods use calling assemblies for help path resolution.
// Callers may be inlined and result in a wrong calling assembly, e.g. FarNet.
// Using NoInlining helps but easy to forget: in debug methods are not inlined
// and look working but in release they may have problems.

namespace FarNet;

/// <summary>
/// Modules implement at least one public not abstract descendant of this class.
/// </summary>
/// <remarks>
/// Any FarNet module implements at least one public not abstract descendant of this class.
/// <para>
/// Normally modules implement one or more actions, descendants of the <see cref="ModuleAction"/> class.
/// When such a module is just installed or updated FarNet loads it and caches module action attributes.
/// Next time when FarNet starts it does not load the module, it reads the information from the cache.
/// This information is enough to show module menu items, register command prefixes, and etc.
/// The module is actually loaded only when a user invokes one of the actions.
/// </para>
/// <para>
/// FarNet creates action class instances and calls <c>Invoke()</c> methods every time when a user invokes actions.
/// Thus, only their static data can be shared between calls.
/// These data can be initialized when the <c>Invoke()</c> or the default constructor is called the first time.
/// If these or other data has to be initialized even before actions then the module host should be used.
/// </para>
/// The module host, descendant of the <see cref="ModuleHost"/>, can be implemented for advanced scenarios.
/// Unlike module actions the host class instance is created, connected and disconnected once.
/// The moment of creation and call of the <see cref="ModuleHost.Connect"/> method depends on the <see cref="ModuleHostAttribute.Load"/> flag.
/// If it is false (default) then the host is loaded and connected only when one of the module actions is invoked.
/// If it is true (preloaded host) then the module is loaded and the host is connected every time.
/// Preloaded hosts should not be used without good reasons.
/// </remarks>
public abstract class BaseModuleItem
{
	/// <summary>
	/// Gets a localized string from .resources files.
	/// </summary>
	/// <returns>Localized string. If a best match is not possible, null is returned.</returns>
	/// <param name="name">The string name.</param>
	/// <remarks>
	/// It gets a string from .resource files depending on the <see cref="IModuleManager.CurrentUICulture"/>.
	/// <para>
	/// The module has to provide .resources files in its directory:
	/// </para>
	/// <ul>
	/// <li>ModuleBaseName.resources (default, English is recommended)</li>
	/// <li>ModuleBaseName.ru.resources (Russian)</li>
	/// <li>ModuleBaseName.de.resources (German)</li>
	/// <li>...</li>
	/// </ul>
	/// <para>
	/// The file "ModuleBaseName.resources" must exist. It normally contains language independent strings
	/// and other strings in a default\fallback language, English more likely. Other files are optional
	/// and can be added at any time. Note that they do not have to repeat language independent strings.
	/// </para>
	/// <para>
	/// See <see cref="CultureInfo"/> about culture names and MSDN about file based resource management.
	/// Use ResGen.exe tool or MSBuild task GenerateResource for binary .resources files generation
	/// from trivial .txt/.restext text files or Visual Studio .resx XML files.
	/// </para>
	/// <para>
	/// If you edit source .resx files in Visual Studio (a very good idea) then ensure they are
	/// either excluded from the project or not compiled and embedded into the output assembly.
	/// </para>
	/// </remarks>
	public string? GetString(string name)
	{
		return Manager.GetString(name);
	}

	/// <summary>
	/// Gets the module manager.
	/// </summary>
	public IModuleManager Manager => _Manager ??= Far.Api.GetModuleManager(GetType());
	IModuleManager? _Manager;

	/// <summary>
	/// Formats the module help topic for <c>HelpTopic</c> properties of various UI classes.
	/// </summary>
	/// <param name="topic">Module help topic name.</param>
	/// <returns>Module help topic path.</returns>
	/// <remarks>
	/// <para>
	/// The help topic should be in a help file in the module directory.
	/// In other cases see <see cref="IFar.ShowHelp"/> for the help path format.
	/// </para>
	/// </remarks>
	public string GetHelpTopic(string topic) //_220609_gf
	{
		var path = Path.GetDirectoryName(GetType().Assembly.Location);
		return "<" + path + "\\>" + topic;
	}

	/// <summary>
	/// Shows the module help topic.
	/// </summary>
	/// <param name="topic">Module help topic.</param>
	/// <remarks>
	/// <para>
	/// The help topic should be in a help file in the module directory.
	/// In other cases use <see cref="IFar.ShowHelp"/>.
	/// </para>
	/// </remarks>
	public void ShowHelpTopic(string topic) //_220609_gf
	{
		var path = Path.GetDirectoryName(GetType().Assembly.Location)!;
		Far.Api.ShowHelp(path, topic, HelpOptions.Path);
	}
}
