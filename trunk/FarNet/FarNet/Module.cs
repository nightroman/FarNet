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
	/// The module manager shared between all module entry classes.
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
		/// The <see cref="BaseModuleEntry.GetString"/> worker.
		/// </summary>
		string GetString(string name);
	}

	/// <summary>
	/// Abstract module entry. Modules implement at least one public not abstract descendant of this class.
	/// </summary>
	/// <remarks>
	/// This class provides localized strings from module .resources files.
	/// </remarks>
	public abstract class BaseModuleEntry
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
			return _ModuleManager.GetString(name);
		}

		/// <summary>
		/// The module manager.
		/// </summary>
		public IModuleManager ModuleManager
		{
			get { return _ModuleManager; }
			set
			{
				if (_ModuleManager != null)
					throw new InvalidOperationException();

				_ModuleManager = value;
			}
		}
		IModuleManager _ModuleManager;
	}

	/// <summary>
	/// Module host attributes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ModuleHostAttribute : Attribute
	{
		/// <summary>
		/// Tells always to load the module and connect the host.
		/// </summary>
		/// <remarks>
		/// If the module host is the only implemented module entry then this flag
		/// should be set to true. Otherwise the host has no chances to be used.
		/// </remarks>
		public bool Load { get; set; }
	}

	/// <summary>
	/// The module host. At most one public descendant can be implemented by a module.
	/// </summary>
	/// <remarks>
	/// In many cases the module tools should be implemented instead of the host
	/// (see predefined <see cref="BaseModuleTool"/> children).
	/// <para>
	/// If the attribute <see cref="ModuleHostAttribute.Load"/> is true then the host is always loaded.
	/// If it is false then the host is loaded only on the first call of any module tool.
	/// A single instance of this class is created for the whole session.
	/// </para>
	/// <para>
	/// This class provides virtual methods called by the core.
	/// Normally the module implements the <see cref="Connect"/> method.
	/// There are a few more optional virtual members that can be implemented when needed.
	/// </para>
	/// </remarks>
	public abstract class ModuleHost : BaseModuleEntry
	{
		/// <summary>
		/// Override this method to process the module connection.
		/// </summary>
		/// <example>
		/// (C#) how to register a command line prefix and a menu command.
		/// <code>
		/// // Register a prefix:
		/// Far.Net.RegisterCommand(this, [name], [prefix], [handler]);
		/// // Register a menu command:
		/// Far.Net.RegisterTool(this, [name], [handler], [options]);
		/// ...
		/// </code>
		/// </example>
		public virtual void Connect()
		{ }

		/// <summary>
		/// Override this method to process module disconnection.
		/// </summary>
		/// <remarks>
		/// NOTE: Don't call Far UI, it is not working on exiting.
		/// Consider to use GUI message boxes if it is absolutely needed.
		/// </remarks>
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public virtual void Disconnect()
		{ }

		/// <summary>
		/// Called before invoking of any module tool.
		/// </summary>
		/// <remarks>
		/// The module may override this method to perform preparation procedures.
		/// Normally this is not needed for a simple module with a single tool.
		/// It is useful when a complex module registers several tools and
		/// wants common steps to be performed by this method.
		/// <para>
		/// NOTE: This method is called only for module tools:
		/// tool <c>Invoke()</c> methods and handlers registered by <c>Register*()</c> methods.
		/// It is not called on events added by a module to editors, viewers, dialogs or panels.
		/// </para>
		/// <para>
		/// Example: PowerShellFar starts loading of the PowerShell engine in a background thread on connection.
		/// This method waits for the engine loading to complete, if needed. All registered PowerShellFar tools
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
	/// Common attributes of any module tool.
	/// </summary>
	public abstract class BaseModuleToolAttribute : Attribute
	{
		/// <summary>
		/// Gets the module tool name to be shown in menus. By default it is the class name.
		/// </summary>
		/// <remarks>
		/// Make sure the module tools have different names.
		/// </remarks>
		public string Name { get; set; }
	}

	/// <summary>
	/// Abstract parent of <see cref="ModuleTool"/>, <see cref="ModuleCommand"/>, <see cref="ModuleEditor"/>, and <see cref="ModuleFiler"/>.
	/// </summary>
	public abstract class BaseModuleTool : BaseModuleEntry
	{
	}

	/// <summary>
	/// Module tool options.
	/// </summary>
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
		/// Show the item in F11 menus.
		/// </summary>
		F11Menus = Panels | Editor | Viewer | Dialog,
		/// <summary>
		/// Show the item in F11 menus and in the disk menu.
		/// </summary>
		AllMenus = F11Menus | Disk,
		/// <summary>
		/// Show the item in F11 menus, the disk menu and the config menu.
		/// </summary>
		AllAreas = AllMenus | Config
	}

	/// <summary>
	/// Module tool attributes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ModuleToolAttribute : BaseModuleToolAttribute
	{
		/// <summary>
		/// Tool options. For a menu tool it is mandatory to specify the menus.
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
		/// Tells to ignore results, e.g. when configuration dialog is cancelled.
		/// </summary>
		public bool Ignore { get; set; }
	}

	/// <summary>
	/// A module tool represented by an item in Far menus.
	/// </summary>
	/// <remarks>
	/// The <see cref="Invoke"/> method has to be implemented.
	/// <para>
	/// For a menu tool it is mandatory to use <see cref="ModuleToolAttribute"/>
	/// and specify menu areas by <see cref="ModuleToolAttribute.Options"/>.
	/// </para>
	/// <include file='doc.xml' path='docs/pp[@name="InvokeLoad"]/*'/>
	/// </remarks>
	public abstract class ModuleTool : BaseModuleTool
	{
		/// <summary>
		/// Tool handler called when its menu item is invoked.
		/// </summary>
		public abstract void Invoke(object sender, ModuleToolEventArgs e);
	}

	/// <summary>
	/// Module command attributes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ModuleCommandAttribute : BaseModuleToolAttribute
	{
		/// <summary>
		/// Command prefix. By default it is the class name.
		/// </summary>
		/// <remarks>
		/// This prefix is only a suggestion, the actual prefix may be changed by a user, so that
		/// if the command uses the prefix then it should use <see cref="ModuleCommandEventArgs.Prefix"/>.
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
		/// <summary>
		/// The actual command prefix: the default or set by a user.
		/// </summary>
		public string Prefix { get; set; }
	}

	/// <summary>
	/// A command called from the command line with a prefix.
	/// </summary>
	/// <remarks>
	/// The <see cref="Invoke"/> method has to be implemented.
	/// <include file='doc.xml' path='docs/pp[@name="InvokeLoad"]/*'/>
	/// </remarks>
	public abstract class ModuleCommand : BaseModuleTool
	{
		/// <summary>
		/// Command handler called from the command line with a prefix.
		/// </summary>
		public abstract void Invoke(object sender, ModuleCommandEventArgs e);
	}

	/// <summary>
	/// Module filer tool attributes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ModuleFilerAttribute : BaseModuleToolAttribute
	{
		/// <include file='doc.xml' path='docs/pp[@name="FileMask"]/*'/>
		public string Mask { get; set; }
		/// <summary>
		/// Tells that the filer also creates files.
		/// </summary>
		public bool Creates { get; set; }
	}

	/// <summary>
	/// Module filer tool event arguments.
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
	/// A module filer tool.
	/// </summary>
	/// <remarks>
	/// The <see cref="Invoke"/> method has to be implemented.
	/// <include file='doc.xml' path='docs/pp[@name="InvokeLoad"]/*'/>
	/// </remarks>
	public abstract class ModuleFiler : BaseModuleTool
	{
		/// <summary>
		/// Filer handler called when a file is opened.
		/// </summary>
		/// <remarks>
		/// It is up to the module how to process a file.
		/// Usually file based modules should represent file data in a panel,
		/// i.e. this methods should be used to open and configure a panel (<see cref="IPanel"/>).
		/// </remarks>
		public abstract void Invoke(object sender, ModuleFilerEventArgs e);
	}

	/// <summary>
	/// Module editor tool attributes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ModuleEditorAttribute : BaseModuleToolAttribute
	{
		/// <include file='doc.xml' path='docs/pp[@name="FileMask"]/*'/>
		public string Mask { get; set; }
	}

	/// <summary>
	/// Module editor tool event arguments.
	/// </summary>
	public class ModuleEditorEventArgs : EventArgs
	{
	}

	/// <summary>
	/// A module editor tool.
	/// </summary>
	/// <remarks>
	/// This tool works with editor events, not with menu commands in editors
	/// (in the latter case use <see cref="ModuleTool"/> configured for editors).
	/// <para>
	/// The <see cref="Invoke"/> method has to be implemented.
	/// </para>
	/// <include file='doc.xml' path='docs/pp[@name="InvokeLoad"]/*'/>
	/// </remarks>
	public abstract class ModuleEditor : BaseModuleTool
	{
		/// <summary>
		/// Editor <see cref="IAnyEditor.Opened"/> handler.
		/// </summary>
		/// <remarks>
		/// This method is called once on opening an editor.
		/// Normally you add your editor event handlers, then they do the jobs.
		/// </remarks>
		/// <example>
		/// See the <c>Modules\TrimSaving</c> module.
		/// It is not just an example, it can be used for real.
		/// </example>
		public abstract void Invoke(object sender, ModuleEditorEventArgs e);
	}

	/// <summary>
	/// A module exception.
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
}
