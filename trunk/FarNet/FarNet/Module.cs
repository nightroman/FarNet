
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FarNet
{
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
		/// Gets a localized string from .resorces files.
		/// </summary>
		/// <returns>Localized string. If a best match is not possible, null is returned.</returns>
		/// <param name="name">String name.</param>
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
		/// from trivial .txt\.restext text files or Visual Studio .resx XML files.
		/// </para>
		/// <para>
		/// If you edit source .resx files in Visual Studio (a very good idea) then ensure they are
		/// either excluded from the project or not compiled and embedded into the output assembly.
		/// </para>
		/// </remarks>
		public string GetString(string name)
		{
			return Manager.GetString(name);
		}
		/// <summary>
		/// Gets the module manager.
		/// </summary>
		public IModuleManager Manager
		{
			get { return _Manager ?? (_Manager = Far.Net.GetModuleManager(GetType())); }
		}
		IModuleManager _Manager;
	}

	/// <summary>
	/// Module exception.
	/// </summary>
	/// <remarks>
	/// If a module throws exceptions then for better diagnostics it is recommended to use this or derived exceptions
	/// in order to be able to distinguish between system, module, and even particular module exceptions.
	/// <para>
	/// Best practice: catch an exception, wrap it by a new module exception with better explanation of a problem and throw the new one.
	/// Wrapped inner exception is not lost: its message and stack are shown, for example by <see cref="IFar.ShowError"/>.
	/// </para>
	/// </remarks>
	[Serializable]
	public class ModuleException : Exception
	{
		///
		public ModuleException() { }
		///
		public ModuleException(string message) : base(message) { }
		///
		public ModuleException(string message, Exception innerException) : base(message, innerException) { }
		///
		protected ModuleException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

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
		public abstract string GetString(string name);
		/// <summary>
		/// Gets the path to the system special folder that is identified by the specified enumeration.
		/// </summary>
		/// <param name="folder">Special folder enumeration.</param>
		/// <param name="create">Tells to create the directory if it does not exist.</param>
		/// <remarks>
		/// Local and roaming data directories are designed for module data and settings files.
		/// NOTE: Names like <b>FarNet.*</b> are reserved for the internal use.
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
		/// Registers the command handler invoked from the command line by its prefix.
		/// </summary>
		/// <param name="id">Unique ID.</param>
		/// <param name="handler">Command handler.</param>
		/// <param name="attribute">Command attribute.</param>
		/// <remarks>
		/// NOTE: Consider to implement the <see cref="ModuleCommand"/> instead.
		/// Dynamic registration is not recommended for standard scenarios.
		/// <include file='doc.xml' path='doc/RegisterModule/*'/>
		/// </remarks>
		public abstract IModuleCommand RegisterModuleCommand(Guid id, ModuleCommandAttribute attribute, EventHandler<ModuleCommandEventArgs> handler);
		/// <summary>
		/// Registers the editor drawer handler.
		/// </summary>
		/// <param name="id">Unique ID.</param>
		/// <param name="handler">Drawer handler.</param>
		/// <param name="attribute">Drawer attribute.</param>
		/// <remarks>
		/// NOTE: Consider to implement the <see cref="ModuleDrawer"/> instead.
		/// Dynamic registration is not recommended for standard scenarios.
		/// <include file='doc.xml' path='doc/RegisterModule/*'/>
		/// </remarks>
		public abstract IModuleDrawer RegisterModuleDrawer(Guid id, ModuleDrawerAttribute attribute, EventHandler<ModuleDrawerEventArgs> handler);
		/// <summary>
		/// Registers the tool handler invoked from one of Far menus.
		/// </summary>
		/// <param name="id">Unique ID.</param>
		/// <param name="handler">Tool handler.</param>
		/// <param name="attribute">Tool attribute.</param>
		/// <remarks>
		/// NOTE: Consider to implement the <see cref="ModuleTool"/> instead.
		/// Dynamic registration is not recommended for standard scenarios.
		/// <include file='doc.xml' path='doc/RegisterModule/*'/>
		/// </remarks>
		public abstract IModuleTool RegisterModuleTool(Guid id, ModuleToolAttribute attribute, EventHandler<ModuleToolEventArgs> handler);
		/// <summary>
		/// Gets the module name.
		/// </summary>
		public abstract string ModuleName { get; }
		/// <summary>
		/// For internal use.
		/// </summary>
		public abstract string StoredUICulture { get; set; }
		/// <summary>
		/// For internal use. Loads the assembly.
		/// </summary>
		public abstract Assembly LoadAssembly(bool connect);
		/// <summary>
		/// For internal use.
		/// </summary>
		public abstract void SaveSettings();
	}

	/// <summary>
	/// Module host attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ModuleHostAttribute : Attribute
	{
		/// <summary>
		/// Tells to load and connect the host always. There should be good reasons for 'true'.
		/// </summary>
		/// <remarks>
		/// If the module host is the only implemented module item then this flag
		/// has to be set to true. Otherwise the host has no chances to be used.
		/// </remarks>
		public bool Load { get; set; }
	}

	/// <summary>
	/// The module host. At most one public descendant can be implemented by a module.
	/// </summary>
	/// <remarks>
	/// In many cases the module actions should be implemented instead of the host
	/// (see predefined descendants of <see cref="ModuleAction"/>).
	/// <para>
	/// If the attribute <see cref="ModuleHostAttribute.Load"/> is true then the host is always loaded.
	/// If it is false then the host is loaded only on the first call of any action.
	/// A single instance of this class is created for the whole session.
	/// </para>
	/// <para>
	/// This class provides virtual methods called by the core.
	/// Normally the module implements the <see cref="Connect"/> method.
	/// There are a few more optional virtual members that can be implemented when needed.
	/// </para>
	/// </remarks>
	public abstract class ModuleHost : BaseModuleItem
	{
		/// <summary>
		/// Override this method to process the module connection.
		/// </summary>
		/// <remarks>
		/// This method is called once.
		/// For standard hosts it is called before creation of the first called module action.
		/// For preloadable hosts it is called immediately after loading of the module assembly and registraion of its actions.
		/// </remarks>
		public virtual void Connect()
		{ }
		/// <summary>
		/// Override this method to process the module disconnection.
		/// </summary>
		/// <remarks>
		/// NOTE: Don't call Far UI, it is not working on exiting.
		/// Consider to use GUI message boxes if it is absolutely needed.
		/// <para>
		/// The host does not have to unregister dynamically registered actions on disconnection.
		/// But added "global" event handlers have to be removed, for example, handlers added to the <see cref="IFar.AnyEditor"/> operator.
		/// </para>
		/// </remarks>
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public virtual void Disconnect()
		{ }
		/// <summary>
		/// Called before invocation of any module action.
		/// </summary>
		/// <remarks>
		/// The module may override this method to perform preparation procedures.
		/// Normally this is not needed for a simple module with a single action.
		/// It is useful when a complex module provides several actions and
		/// wants common steps to be performed by this method.
		/// <para>
		/// NOTE: This method is called only for module actions:
		/// <c>Invoke()</c> methods and handlers registered by <c>Register*()</c> methods.
		/// It is not called on events added by a module to editors, viewers, dialogs or panels.
		/// </para>
		/// <para>
		/// Example: PowerShellFar starts loading of the PowerShell engine in a background thread on connection.
		/// This method waits for the engine loading to complete, if needed. Registered PowerShellFar actions
		/// simply assume that the engine is already loaded. But editor event handlers still have to care.
		/// </para>
		/// </remarks>
		public virtual void Invoking()
		{ }
		/// <summary>
		/// Can the module exit now?
		/// </summary>
		/// <remarks>
		/// This method is normally called internally by the <see cref="IFar.Quit"/>.
		/// The module can override this to perform preliminary checks before exit.
		/// Note that final exit actions should be performed in <see cref="Disconnect"/>.
		/// <para>
		/// It is allowed to return false but this option should be used sparingly,
		/// there must be really good reasons to disturb normal exiting process.
		/// The most important reason is that a user really wants that.
		/// </para>
		/// </remarks>
		/// <returns>True if the module is ready to exit.</returns>
		public virtual bool CanExit()
		{
			return true;
		}
	}

	/// <summary>
	/// Any action attribute parameters.
	/// </summary>
	public abstract class ModuleActionAttribute : Attribute, ICloneable
	{
		/// <summary>
		/// The action name shown in menus. It is mandatory to specify.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the module uses this name itself, for example as message boxes titles, then define this text
		/// as a public const string in a class, then use its name as the value of this attribute parameter.
		/// </para>
		/// </remarks>
		public string Name { get; set; }
		/// <summary>
		/// Tells to use the <see cref="Name"/> as the resource name of the localized string.
		/// </summary>
		/// <remarks>
		/// Restart Far after changing the current Far language or the module culture
		/// to make sure that this and other action names are updated from resources.
		/// </remarks>
		public bool Resources { get; set; }
		///
		public object Clone() { return MemberwiseClone(); }
	}

	/// <summary>
	/// Abstract parent of <see cref="ModuleTool"/>, <see cref="ModuleCommand"/>, <see cref="ModuleEditor"/>.
	/// </summary>
	public abstract class ModuleAction : BaseModuleItem
	{
	}

	/// <summary>
	/// Module command attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ModuleCommandAttribute : ModuleActionAttribute
	{
		/// <summary>
		/// The command prefix. It is mandatory to specify a not empty value.
		/// </summary>
		/// <remarks>
		/// This prefix is only a suggestion, the actual prefix is configured by a user.
		/// </remarks>
		public string Prefix { get; set; }
	}

	/// <summary>
	/// Arguments of a module command event.
	/// </summary>
	public class ModuleCommandEventArgs : EventArgs
	{
		/// <summary>
		/// The command text to be processed by the module.
		/// </summary>
		public string Command { get; set; }
		/// <summary>
		/// The macro area where the command is called from by <c>CallPlugin()</c> (see FarNet Readme.txt)
		/// </summary>
		public MacroArea MacroArea { get; set; }
		/// <summary>
		/// Tells to ignore the call and allows alternative actions.
		/// </summary>
		/// <remarks>
		/// This flag is used when the command is called from a macro.
		/// <para>
		/// A handler sets this to true to tell that nothing is done and
		/// it makes sense for a caller to perfom an alternative action.
		/// </para>
		/// <para>
		/// Note: this is not the case when processing has started and failed;
		/// the handler should either throw an exception or keep this value as false:
		/// fallback actions make no sense, the problems have to be resolved instead.
		/// </para>
		/// </remarks>
		public bool Ignore { get; set; }
	}

	/// <summary>
	/// A command called by its prefix from command lines and macros.
	/// </summary>
	/// <remarks>
	/// The <see cref="Invoke"/> method has to be implemented.
	/// <para>
	/// Commands are called by their prefixes from command lines: the panel command line and user menu and file association commands.
	/// Macros call commands by <c>CallPlugin()</c> (see FarNet Readme.txt).
	/// </para>
	/// <para>
	/// It is mandatory to use <see cref="ModuleCommandAttribute"/> and specify the <see cref="ModuleActionAttribute.Name"/>
	/// and the default command prefix <see cref="ModuleCommandAttribute.Prefix"/>.
	/// </para>
	/// <include file='doc.xml' path='doc/ActionGuid/*'/>
	/// </remarks>
	public abstract class ModuleCommand : ModuleAction
	{
		/// <summary>
		/// Command handler called from the command line with a prefix.
		/// </summary>
		public abstract void Invoke(object sender, ModuleCommandEventArgs e);
	}

	/// <summary>
	/// Module editor action attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ModuleEditorAttribute : ModuleActionAttribute
	{
		/// <include file='doc.xml' path='doc/FileMask/*'/>
		public string Mask { get; set; }
	}

	/// <summary>
	/// Module editor drawer attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ModuleDrawerAttribute : ModuleActionAttribute
	{
		/// <include file='doc.xml' path='doc/FileMask/*'/>
		public string Mask { get; set; }
		/// <summary>
		/// Color priority.
		/// </summary>
		public int Priority { get; set; }
	}

	/// <summary>
	/// Module editor event arguments.
	/// </summary>
	public class ModuleEditorEventArgs : EventArgs
	{
	}

	/// <summary>
	/// Module drawer event arguments.
	/// </summary>
	public class ModuleDrawerEventArgs : EventArgs
	{
		///
		public ModuleDrawerEventArgs(ICollection<EditorColor> colors, IList<ILine> lines, int startChar, int endChar)
		{
			Colors = colors;
			Lines = lines;
			StartChar = startChar;
			EndChar = endChar;
		}
		/// <summary>
		/// Gets the result color collection. A drawer adds colors to it.
		/// </summary>
		public ICollection<EditorColor> Colors { get; private set; }
		/// <summary>
		/// Gets the lines to get colors for. A drawer should not change anything.
		/// </summary>
		public IList<ILine> Lines { get; private set; }
		/// <summary>
		/// Index of the first character.
		/// </summary>
		public int StartChar { get; private set; }
		/// <summary>
		/// Index of the character after the last.
		/// </summary>
		public int EndChar { get; private set; }
	}

	/// <summary>
	/// A module editor action.
	/// </summary>
	/// <remarks>
	/// This action deals with an editor opening, not with menu commands in editors
	/// (in the latter case use <see cref="ModuleTool"/> configured for editors).
	/// <para>
	/// The <see cref="Invoke"/> method has to be implemented.
	/// </para>
	/// <para>
	/// It is mandatory to use <see cref="ModuleEditorAttribute"/> and specify the <see cref="ModuleActionAttribute.Name"/>.
	/// The optional default file mask is defined as <see cref="ModuleEditorAttribute.Mask"/>.
	/// </para>
	/// <include file='doc.xml' path='doc/ActionGuid/*'/>
	/// </remarks>
	public abstract class ModuleEditor : ModuleAction
	{
		/// <summary>
		/// Editor <see cref="IEditorBase.Opened"/> handler.
		/// </summary>
		/// <remarks>
		/// This method is called once on opening an editor.
		/// Normally it adds editor event handlers, then they do the jobs.
		/// </remarks>
		/// <example>
		/// See the <c>TrimSaving</c> module.
		/// It is not just an example, it can be used for real.
		/// </example>
		public abstract void Invoke(object sender, ModuleEditorEventArgs e);
	}

	/// <summary>
	/// A module drawer action.
	/// </summary>
	/// <remarks>
	/// This action is called on editor drawing in order to get colors for the specified lines.
	/// <para>
	/// The <see cref="Invoke"/> method has to be implemented.
	/// It should work as fast as possible because it is called frequently.
	/// Its goal is to fill the color collection, it should not change anything.
	/// </para>
	/// <para>
	/// It is mandatory to use <see cref="ModuleDrawerAttribute"/> and specify the <see cref="ModuleActionAttribute.Name"/>.
	/// The optional default file mask is defined as <see cref="ModuleDrawerAttribute.Mask"/>
	/// and the default color priority <see cref="ModuleDrawerAttribute.Priority"/>.
	/// </para>
	/// <include file='doc.xml' path='doc/ActionGuid/*'/>
	/// </remarks>
	public abstract class ModuleDrawer : ModuleAction
	{
		/// <summary>
		/// Gets colors for the specified editor lines.
		/// </summary>
		public abstract void Invoke(object sender, ModuleDrawerEventArgs e);
	}

	/// <summary>
	/// Module tool options, combination of flags.
	/// </summary>
	/// <remarks>
	/// Choose the flags carefully, include areas where the tool really works.
	/// Nobody wants to have their plugin menus polluted by not working items.
	/// </remarks>
	[Flags]
	public enum ModuleToolOptions
	{
		/// <summary>
		/// None.
		/// </summary>
		None,
		/// <summary>
		/// Show the item in the config menu and call it from other specifiled menus by [ShiftF9].
		/// </summary>
		Config = 1 << 0,
		/// <summary>
		/// Show the item in the disk menu.
		/// </summary>
		Disk = 1 << 1,
		/// <summary>
		/// Show the item in the editor plugin menu.
		/// </summary>
		Editor = 1 << 2,
		/// <summary>
		/// Show the item in the panel plugin menu.
		/// </summary>
		Panels = 1 << 3,
		/// <summary>
		/// Show the item in the viewer plugin menu.
		/// </summary>
		Viewer = 1 << 4,
		/// <summary>
		/// Show the item in the dialog plugin menu.
		/// </summary>
		Dialog = 1 << 5,
		/// <summary>
		/// Show the item in all [F11] menus (Panels | Editor | Viewer | Dialog).
		/// </summary>
		F11Menus = Panels | Editor | Viewer | Dialog,
		/// <summary>
		/// Show the item in [F11] menus and in the disk menu (F11Menus | Disk).
		/// </summary>
		AllMenus = F11Menus | Disk,
		/// <summary>
		/// Show the item in [F11] menus, the disk menu and the config menu (AllMenus | Config).
		/// </summary>
		AllAreas = AllMenus | Config
	}

	/// <summary>
	/// Module tool action attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ModuleToolAttribute : ModuleActionAttribute
	{
		/// <summary>
		/// Tool options. It is mandatory to specify at least one menu or other area.
		/// </summary>
		public ModuleToolOptions Options { get; set; }
	}

	/// <summary>
	/// Arguments of a module tool event.
	/// </summary>
	/// <remarks>
	/// This event is called from plugin, disk or configuration menus.
	/// </remarks>
	public sealed class ModuleToolEventArgs : EventArgs
	{
		/// <summary>
		/// Where it is called from.
		/// </summary>
		public ModuleToolOptions From { get; set; }
		/// <summary>
		/// Tells to ignore results, for example when a configuration dialog is canceled.
		/// </summary>
		public bool Ignore { get; set; }
		/// <summary>
		/// Gets true if the event is called from the left disk menu.
		/// </summary>
		public bool IsLeft { get; set; }
	}

	/// <summary>
	/// A module tool represented by an item in Far menus.
	/// </summary>
	/// <remarks>
	/// The <see cref="Invoke"/> method has to be implemented.
	/// <para>
	/// It is mandatory to use <see cref="ModuleToolAttribute"/> and specify the <see cref="ModuleActionAttribute.Name"/>
	/// and the menu areas <see cref="ModuleToolAttribute.Options"/>.
	/// </para>
	/// <include file='doc.xml' path='doc/ActionGuid/*'/>
	/// </remarks>
	public abstract class ModuleTool : ModuleAction
	{
		/// <summary>
		/// Tool handler called when its menu item is invoked.
		/// </summary>
		public abstract void Invoke(object sender, ModuleToolEventArgs e);
	}

	/// <summary>
	/// Module item kinds.
	/// </summary>
	public enum ModuleItemKind
	{
		///
		None,
		///
		Host,
		///
		Command,
		///
		Editor,
		///
		Tool,
		///
		Drawer
	}

	/// <summary>
	/// Module action runtime representation.
	/// </summary>
	/// <remarks>
	/// Any registered module action has its runtime representation, one of this inderface descendants.
	/// These representation interfaces are not directly related to action classes or handlers, they only represent them.
	/// <para>
	/// Action representations can be requested by their IDs by <see cref="IFar.GetModuleAction"/>.
	/// </para>
	/// </remarks>
	public interface IModuleAction
	{
		/// <summary>
		/// Gets the action ID.
		/// </summary>
		Guid Id { get; }
		/// <summary>
		/// Gets the action name.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Gets the action kind.
		/// </summary>
		ModuleItemKind Kind { get; }
		/// <summary>
		/// Gets the module manager.
		/// </summary>
		IModuleManager Manager { get; }
		/// <summary>
		/// Unregisters the module action dynamically.
		/// </summary>
		/// <remarks>
		/// Normally it is used for temporary actions dynamically registered by <c>Register*()</c>.
		/// <para>
		/// Note that module hosts on disconnection does not have to unregister registered actions.
		/// </para>
		/// </remarks>
		void Unregister();
	}

	/// <summary>
	/// Module command runtime representation.
	/// </summary>
	/// <remarks>
	/// It represents an auto registered <see cref="ModuleCommand"/> or a command registered by <see cref="IModuleManager.RegisterModuleCommand"/>.
	/// It can be accessed by <see cref="IFar.GetModuleAction"/> from any module.
	/// </remarks>
	public interface IModuleCommand : IModuleAction
	{
		/// <summary>
		/// Processes the command event.
		/// </summary>
		void Invoke(object sender, ModuleCommandEventArgs e);
		/// <summary>
		/// Gets the actual command prefix.
		/// </summary>
		string Prefix { get; }
		/// <summary>
		/// Gets the default prefix.
		/// </summary>
		string DefaultPrefix { get; }
		/// <summary>
		/// For internal use.
		/// </summary>
		void ResetPrefix(string value);
	}

	/// <summary>
	/// Module editor runtime representation.
	/// </summary>
	/// <remarks>
	/// It represents an auto registered <see cref="ModuleEditor"/> actions.
	/// </remarks>
	public interface IModuleEditor : IModuleAction
	{
		/// <summary>
		/// Processes the editor event.
		/// </summary>
		void Invoke(object sender, ModuleEditorEventArgs e);
		/// <summary>
		/// Gets the actual file mask.
		/// </summary>
		string Mask { get; }
		/// <summary>
		/// Gets the default file mask.
		/// </summary>
		string DefaultMask { get; }
		/// <summary>
		/// For internal use.
		/// </summary>
		void ResetMask(string value);
	}

	/// <summary>
	/// Module drawer runtime representation.
	/// </summary>
	/// <remarks>
	/// It represents an auto registered <see cref="ModuleDrawer"/> or a drawer registered by <see cref="IModuleManager.RegisterModuleDrawer"/>.
	/// </remarks>
	public interface IModuleDrawer : IModuleAction
	{
		/// <summary>
		/// Returns the drawer handler.
		/// </summary>
		EventHandler<ModuleDrawerEventArgs> Handler();
		/// <summary>
		/// Gets the actual file mask.
		/// </summary>
		string Mask { get; }
		/// <summary>
		/// Gets the default file mask.
		/// </summary>
		string DefaultMask { get; }
		/// <summary>
		/// For internal use.
		/// </summary>
		void ResetMask(string value);
		/// <summary>
		/// Gets the actual priority.
		/// </summary>
		int Priority { get; }
		/// <summary>
		/// Gets the default priority.
		/// </summary>
		int DefaultPriority { get; }
		/// <summary>
		/// For internal use.
		/// </summary>
		void ResetPriority(int value);
	}

	/// <summary>
	/// Module tool runtime representation.
	/// </summary>
	/// <remarks>
	/// It represents an auto registered <see cref="ModuleTool"/> or a tool registered by <see cref="IModuleManager.RegisterModuleTool"/>.
	/// It can be accessed by <see cref="IFar.GetModuleAction"/> from any module.
	/// </remarks>
	public interface IModuleTool : IModuleAction
	{
		/// <summary>
		/// Processes the tool event.
		/// </summary>
		void Invoke(object sender, ModuleToolEventArgs e);
		/// <summary>
		/// Gets the actual tool options.
		/// </summary>
		ModuleToolOptions Options { get; }
		/// <summary>
		/// Gets the default tool options.
		/// </summary>
		ModuleToolOptions DefaultOptions { get; }
		/// <summary>
		/// For internal use.
		/// </summary>
		void ResetOptions(ModuleToolOptions value);
	}

}
