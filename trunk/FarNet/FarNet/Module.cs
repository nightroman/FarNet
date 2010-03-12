/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Globalization;
using System.IO;
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
			return _Manager.GetString(name);
		}

		/// <summary>
		/// The module manager.
		/// </summary>
		public IModuleManager Manager
		{
			get { return _Manager; }
			set
			{
				if (_Manager != null)
					throw new InvalidOperationException();

				_Manager = value;
			}
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
	public interface IModuleManager
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
		CultureInfo CurrentUICulture { get; set; }
		/// <summary>
		/// The <see cref="BaseModuleItem.GetString"/> worker.
		/// </summary>
		string GetString(string name);
		/// <summary>
		/// Unregisters the module in critical cases.
		/// </summary>
		/// <remarks>
		/// This method should be called only on fatal errors and similar cases.
		/// </remarks>
		void Unregister();
		/// <summary>
		/// Registers the command handler invoked from the command line by its prefix.
		/// </summary>
		/// <param name="id">Unique command ID.</param>
		/// <param name="handler">Command handler.</param>
		/// <param name="attribute">Command attribute.</param>
		/// <remarks>
		/// NOTE: Consider to implement the <see cref="ModuleCommand"/> instead.
		/// Dynamic registration is not recommended for standard scenarios.
		/// <include file='doc.xml' path='docs/pp[@name="RegisterModule"]/*'/>
		/// </remarks>
		IModuleCommand RegisterModuleCommand(Guid id, ModuleCommandAttribute attribute, EventHandler<ModuleCommandEventArgs> handler);
		/// <summary>
		/// Registers the file handler invoked for a file. See <see cref="ModuleFilerEventArgs"/>.
		/// </summary>
		/// <param name="id">Unique filer ID.</param>
		/// <param name="handler">Filer handler.</param>
		/// <param name="attribute">Filer attribute.</param>
		/// <remarks>
		/// NOTE: Consider to implement the <see cref="ModuleFiler"/> instead.
		/// Dynamic registration is not recommended for standard scenarios.
		/// <include file='doc.xml' path='docs/pp[@name="RegisterModule"]/*'/>
		/// </remarks>
		IModuleFiler RegisterModuleFiler(Guid id, ModuleFilerAttribute attribute, EventHandler<ModuleFilerEventArgs> handler);
		/// <summary>
		/// Registers the tool handler invoked from one of Far menus.
		/// </summary>
		/// <param name="id">Unique tool ID.</param>
		/// <param name="handler">Tool handler.</param>
		/// <param name="attribute">Tool attribute.</param>
		/// <remarks>
		/// NOTE: Consider to implement the <see cref="ModuleTool"/> instead.
		/// Dynamic registration is not recommended for standard scenarios.
		/// <include file='doc.xml' path='docs/pp[@name="RegisterModule"]/*'/>
		/// </remarks>
		IModuleTool RegisterModuleTool(Guid id, ModuleToolAttribute attribute, EventHandler<ModuleToolEventArgs> handler);
		/// <summary>
		/// Opens the registry key where the module may keep its local data like permanent settings.
		/// </summary>
		/// <param name="name">Name or path of the key to open. If it is null or empty then the root key is opened.</param>
		/// <param name="writable">Set to true if you need write access to the key.</param>
		/// <returns>The requested key or null if the key for reading does not exist.</returns>
		/// <remarks>
		/// The returned key has to be disposed after use by <c>Dispose()</c>.
		/// <para>
		/// For the Far Manager host the root module key in the Windows registry is <c>...\Plugins\FarNet.Modules\Module.dll</c>.
		/// </para>
		/// </remarks>
		IRegistryKey OpenRegistryKey(string name, bool writable);
		/// <summary>
		/// Gets the module name.
		/// </summary>
		string ModuleName { get; }
		/// <summary>
		/// For internal use.
		/// </summary>
		string StoredUICulture { get; set; }
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
	/// Abstract parent of <see cref="ModuleTool"/>, <see cref="ModuleCommand"/>, <see cref="ModuleEditor"/>, and <see cref="ModuleFiler"/>.
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
		/// The command text to process.
		/// </summary>
		public string Command { get; set; }
	}

	/// <summary>
	/// A command called from the command line with a prefix.
	/// </summary>
	/// <remarks>
	/// The <see cref="Invoke"/> method has to be implemented.
	/// <para>
	/// It is mandatory to use <see cref="ModuleCommandAttribute"/> and specify the <see cref="ModuleActionAttribute.Name"/>
	/// and the default command prefix <see cref="ModuleCommandAttribute.Prefix"/>.
	/// </para>
	/// <include file='doc.xml' path='docs/pp[@name="Guid"]/*'/>
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
		/// <include file='doc.xml' path='docs/pp[@name="FileMask"]/*'/>
		public string Mask { get; set; }
	}

	/// <summary>
	/// Module editor action event arguments.
	/// </summary>
	public class ModuleEditorEventArgs : EventArgs
	{
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
	/// <include file='doc.xml' path='docs/pp[@name="Guid"]/*'/>
	/// </remarks>
	public abstract class ModuleEditor : ModuleAction
	{
		/// <summary>
		/// Editor <see cref="IAnyEditor.Opened"/> handler.
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
	/// Module filer action attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ModuleFilerAttribute : ModuleActionAttribute
	{
		/// <include file='doc.xml' path='docs/pp[@name="FileMask"]/*'/>
		public string Mask { get; set; }
		/// <summary>
		/// Tells that the filer also creates files.
		/// </summary>
		public bool Creates { get; set; }
	}

	/// <summary>
	/// Module filer action event arguments.
	/// </summary>
	/// <remarks>
	/// A handler is called to open a <see cref="IPanel"/> which emulates a file system based on a file.
	/// If a file is unknown a handler should do nothing.
	/// </remarks>
	public sealed class ModuleFilerEventArgs : EventArgs
	{
		/// <summary>
		/// Full name of a file including the path.
		/// If it is empty then a handler is called to create a new file [ShiftF1].
		/// In any case a handler opens <see cref="IPanel"/> or ignores this call.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Data from the beginning of the file used to detect the file type.
		/// Use this stream in a handler only or copy the data for later use.
		/// </summary>
		public Stream Data { get; set; }
		/// <summary>
		/// Combination of the operation mode flags.
		/// </summary>
		public OperationModes Mode { get; set; }
	}

	/// <summary>
	/// A module filer action.
	/// </summary>
	/// <remarks>
	/// The <see cref="Invoke"/> method has to be implemented.
	/// <para>
	/// It is mandatory to use <see cref="ModuleFilerAttribute"/> and specify the <see cref="ModuleActionAttribute.Name"/>.
	/// The optional default file mask is defined as <see cref="ModuleFilerAttribute.Mask"/>.
	/// </para>
	/// <include file='doc.xml' path='docs/pp[@name="Guid"]/*'/>
	/// </remarks>
	public abstract class ModuleFiler : ModuleAction
	{
		/// <summary>
		/// Filer handler called when a file is opened.
		/// </summary>
		/// <remarks>
		/// It is up to the module how to process a file.
		/// But usually filers represent file data in a panel,
		/// so that this method is used to open and configure a panel (<see cref="IPanel"/>).
		/// </remarks>
		public abstract void Invoke(object sender, ModuleFilerEventArgs e);
	}

	/// <summary>
	/// Module tool options, combination of flags.
	/// </summary>
	/// <remarks>
	/// Choose the flags carefully, do not include areas where the tool is not supposed to work.
	/// Nobody wants to see their tool menus polluted by items that do not actually work.
	/// </remarks>
	[Flags]
	public enum ModuleToolOptions
	{
		/// <summary>
		/// None.
		/// </summary>
		None,
		/// <summary>
		/// Show the item in the config menu.
		/// </summary>
		Config = 1 << 0,
		/// <summary>
		/// Show the item in the disk menu.
		/// </summary>
		Disk = 1 << 1,
		/// <summary>
		/// Show the item in the menu called from the editor.
		/// </summary>
		Editor = 1 << 2,
		/// <summary>
		/// Show the item in the menu called from the panels.
		/// </summary>
		Panels = 1 << 3,
		/// <summary>
		/// Show the item in the menu called from the viewer.
		/// </summary>
		Viewer = 1 << 4,
		/// <summary>
		/// Show the item in the menu called from dialogs.
		/// </summary>
		Dialog = 1 << 5,
		/// <summary>
		/// Show the item in all F11 menus (Panels | Editor | Viewer | Dialog).
		/// </summary>
		F11Menus = Panels | Editor | Viewer | Dialog,
		/// <summary>
		/// Show the item in F11 menus and in the disk menu (F11Menus | Disk).
		/// </summary>
		AllMenus = F11Menus | Disk,
		/// <summary>
		/// Show the item in F11 menus, the disk menu and the config menu (AllMenus | Config).
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
	/// This event is normally called from the Far plugin, disk or configuration menus.
	/// </remarks>
	public sealed class ModuleToolEventArgs : EventArgs
	{
		/// <summary>
		/// Where it is called from.
		/// </summary>
		public ModuleToolOptions From { get; set; }
		/// <summary>
		/// Tells to ignore results, for example when a configuration dialog is cancelled.
		/// </summary>
		public bool Ignore { get; set; }
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
	/// <include file='doc.xml' path='docs/pp[@name="Guid"]/*'/>
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
		Filer,
		///
		Tool
	}

	/// <summary>
	/// Module action runtime representation.
	/// </summary>
	/// <remarks>
	/// Any registered module action has its runtime representation, one of this inderface descendants.
	/// These representation interfaces are not directly related to action classes or handlers, they only represent them.
	/// <para>
	/// Action representations can be requested by their IDs by
	/// <see cref="IFar.GetModuleCommand"/>, <see cref="IFar.GetModuleFiler"/>, and <see cref="IFar.GetModuleTool"/>.
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
		/// Gets the module name.
		/// </summary>
		string ModuleName { get; }
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
	/// It can be accessed by <see cref="IFar.GetModuleCommand"/> from any module.
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
	/// Module filer runtime representation.
	/// </summary>
	/// <remarks>
	/// It represents an auto registered <see cref="ModuleFiler"/> or a filer registered by <see cref="IModuleManager.RegisterModuleFiler"/>.
	/// It can be accessed by <see cref="IFar.GetModuleFiler"/> from any module.
	/// </remarks>
	public interface IModuleFiler : IModuleAction
	{
		/// <summary>
		/// Processes the filer event.
		/// </summary>
		void Invoke(object sender, ModuleFilerEventArgs e);
		/// <summary>
		/// Gets the file mask.
		/// </summary>
		string Mask { get; }
		/// <summary>
		/// Gets true if the filer also creates files.
		/// </summary>
		bool Creates { get; }
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
	/// Module tool runtime representation.
	/// </summary>
	/// <remarks>
	/// It represents an auto registered <see cref="ModuleTool"/> or a tool registered by <see cref="IModuleManager.RegisterModuleTool"/>.
	/// It can be accessed by <see cref="IFar.GetModuleTool"/> from any module.
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
		/// Gets the menu hotkey.
		/// </summary>
		string Hotkey { get; }
		/// <summary>
		/// Gets the default tool options.
		/// </summary>
		ModuleToolOptions DefaultOptions { get; }
		/// <summary>
		/// For internal use.
		/// </summary>
		void ResetHotkey(string value);
		/// <summary>
		/// For internal use.
		/// </summary>
		void ResetOptions(ModuleToolOptions value);
	}

}
