/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FarNet
{
	/// <summary>
	/// Base module class, the bridge between FarNet and the module tools.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is the base class of preloadable modules and not preloadable
	/// <see cref="ModuleTool"/>, <see cref="ModuleCommand"/>, <see cref="ModuleEditor"/> and <see cref="ModuleFiler"/>.
	/// </para>
	/// <para>
	/// It exposes the global <see cref="Far"/> instance and provides virtual methods called by the core.
	/// Normally a direct child implements the <see cref="Connect"/> method and registers its tools in it;
	/// grand children (derived from <c>Module*</c>) implement their <c>Invoke</c> methods.
	/// There are a few more optional virtual members that can be implemented when needed.
	/// </para>
	/// <para>
	/// Any direct child of this class is preloadable and makes other tools in the same assembly preloadable.
	/// </para>
	/// <para>
	/// For a module that provides a single top level operation use <see cref="ModuleTool"/>, <see cref="ModuleCommand"/>, or <see cref="ModuleFiler"/>.
	/// For a module that only installs editor events for all or specified file types use <see cref="ModuleEditor"/>.
	/// These modules are normally not preloadable and slightly easier to implement.
	/// </para>
	/// </remarks>
	public class BaseModule
	{
		ResourceManager Resource;

		/// <summary>
		/// Denies instances of this class.
		/// </summary>
		protected BaseModule()
		{ }

		/// <summary>
		/// Gets the object which exposes FarNet methods and creates other FarNet objects.
		/// </summary>
		/// <remarks>
		/// This object is really the main gateway to absolutely all FarNet API.
		/// It exposes properties and methods for direct use and a few Create*
		/// methods that create other FarNet objects with their own members.
		/// <para>
		/// NOTE: It is set internally once and should never be changed.
		/// </para>
		/// </remarks>
		public IFar Far { get; set; }

		/// <include file='doc.xml' path='docs/pp[@name="Connect"]/*'/>
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
		/// Gets the module or its menu item name. By default it is the module class name.
		/// </summary>
		/// <remarks>
		/// If you override it (usually in <see cref="ModuleTool"/>, <see cref="ModuleCommand"/>, or <see cref="ModuleFiler"/>)
		/// and use several module tools in the assembly then make sure that names do not clash.
		/// </remarks>
		public virtual string Name
		{
			get
			{
				return GetType().FullName;
			}
		}

		/// <summary>
		/// Gets or sets the current UI culture.
		/// </summary>
		/// <remarks>
		/// Method <see cref="GetString"/> gets localized strings depending exactly on this property value.
		/// This property is set internally to the Far UI culture and normally you do not care of it.
		/// But you may want to set it yourself:
		/// <ul>
		/// <li>To ensure it is the same as the current Far UI culture after its changes.</li>
		/// <li>To use the culture different from the current Far UI culture (for testing or whatever).</li>
		/// <li>To use the culture which is not even known to Far itself (there is no such a .LNG file).</li>
		/// </ul>
		/// </remarks>
		public CultureInfo CurrentUICulture
		{
			get
			{
				if (_CurrentUICulture == null)
					_CurrentUICulture = Far.GetCurrentUICulture(false);

				return _CurrentUICulture;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				_CurrentUICulture = value;
			}
		}
		CultureInfo _CurrentUICulture;

		/// <summary>
		/// Called before invoking of a command.
		/// </summary>
		/// <remarks>
		/// The module may override it to perform some preparations before invoking.
		/// Example: PowerShellFar may wait for the PowerShell engine loading to complete.
		/// </remarks>
		public virtual void Invoking()
		{ }

		/// <summary>
		/// Gets a localized string from .resorces files.
		/// </summary>
		/// <returns>Localized string. If a best match is not possible, null is returned.</returns>
		/// <param name="name">String name.</param>
		/// <remarks>
		/// It gets a string from .resource files depending on the <see cref="CurrentUICulture"/>.
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
			if (Resource == null)
				Resource = Far.Zoo.CreateFileBasedResourceManager(this);
			
			return Resource.GetString(name, CurrentUICulture);
		}

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
	/// Tool options.
	/// </summary>
	[Flags]
	public enum ToolOptions
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
	/// Arguments of a module tool event.
	/// This event normally happens in the Far plugins menu.
	/// </summary>
	public sealed class ToolEventArgs : EventArgs
	{
		///
		public ToolEventArgs(ToolOptions from)
		{
			_From = from;
		}
		/// <summary>
		/// Where it is called from.
		/// </summary>
		public ToolOptions From
		{
			get { return _From; }
		}
		ToolOptions _From;
		/// <summary>
		/// Tells to ignore results, e.g. when configuration dialog is cancelled.
		/// </summary>
		public bool Ignore { get; set; }
	}

	/// <summary>
	/// Base class of a tool represented by an item in Far menus.
	/// </summary>
	/// <remarks>
	/// It is enough to implement <see cref="Invoke"/> method only.
	/// Override other properties and methods as needed.
	/// You may derive any number of such classes.
	/// <include file='doc.xml' path='docs/pp[@name="InvokeLoad"]/*'/>
	/// </remarks>
	public abstract class ModuleTool : BaseModule
	{
		/// <summary>
		/// Tool handler called when its menu item is invoked.
		/// </summary>
		public abstract void Invoke(object sender, ToolEventArgs e);

		/// <summary>
		/// Tool options. By default the tool is shown in all menus.
		/// Override to specify only needed areas.
		/// </summary>
		public virtual ToolOptions Options
		{
			get { return ToolOptions.AllAreas; }
		}
	}

	/// <summary>
	/// Arguments of a module command event.
	/// </summary>
	public class CommandEventArgs : EventArgs
	{
		///
		public CommandEventArgs(string command)
		{
			_command = command;
		}
		string _command;
		/// <summary>
		/// Command to process.
		/// </summary>
		public string Command
		{
			get { return _command; }
		}
	}

	/// <summary>
	/// Base class of a command called from the command line with a prefix.
	/// </summary>
	/// <remarks>
	/// You have to implement <see cref="Invoke"/> and provide <see cref="Prefix"/>.
	/// Override other properties and methods as needed.
	/// You may derive any number of such classes.
	/// <include file='doc.xml' path='docs/pp[@name="InvokeLoad"]/*'/>
	/// </remarks>
	public abstract class ModuleCommand : BaseModule
	{
		/// <summary>
		/// Command handler called from the command line with a prefix.
		/// </summary>
		public abstract void Invoke(object sender, CommandEventArgs e);

		/// <summary>
		/// Command prefix. By default it is the class name, override <c>get</c> for another one.
		/// But it is only a suggestion, actual prefix may be changed by a user, so that
		/// if the module uses the prefix itself then override <c>set</c> too.
		/// </summary>
		public virtual string Prefix
		{
			get { return this.GetType().Name; }
			set { }
		}
	}

	/// <summary>
	/// Arguments for a handler registered by <see cref="IFar.RegisterFiler"/>.
	/// A handler is called to open a <see cref="IPluginPanel"/> which emulates a file system based on a file.
	/// If a file is unknown a handler should do nothing.
	/// </summary>
	public sealed class FilerEventArgs : EventArgs
	{
		///
		public FilerEventArgs(string name, Stream data, OperationModes mode)
		{
			_Name = name;
			_Data = data;
			_Mode = mode;
		}
		string _Name;
		/// <summary>
		/// Full name of a file including the path.
		/// If it is empty then a handler is called to create a new file [ShiftF1].
		/// In any case a handler opens <see cref="IPluginPanel"/> or ignores this call.
		/// </summary>
		public string Name
		{
			get { return _Name; }
		}
		Stream _Data;
		/// <summary>
		/// Data from the beginning of the file used to detect the file type.
		/// Use this stream in a handler only or copy the data for later use.
		/// </summary>
		public Stream Data
		{
			get { return _Data; }
		}
		OperationModes _Mode;
		/// <summary>
		/// Combination of the operation mode flags.
		/// </summary>
		public OperationModes Mode
		{
			get { return _Mode; }
		}
	}

	/// <summary>
	/// Base class of a module filer.
	/// </summary>
	/// <remarks>
	/// It is enough to implement <see cref="Invoke"/> method only.
	/// Override other properties and methods as needed.
	/// You may derive any number of such classes.
	/// <include file='doc.xml' path='docs/pp[@name="InvokeLoad"]/*'/>
	/// </remarks>
	public abstract class ModuleFiler : BaseModule
	{
		/// <summary>
		/// Filer handler called when a file is opened.
		/// </summary>
		/// <remarks>
		/// It is up to the module how to process a file.
		/// Usually file based modules should represent file data in a panel,
		/// i.e. this methods should be used to open and configure a panel (<see cref="IPluginPanel"/>).
		/// </remarks>
		public abstract void Invoke(object sender, FilerEventArgs e);

		/// <include file='doc.xml' path='docs/pp[@name="FileMask"]/*'/>
		public virtual string Mask
		{
			get { return string.Empty; }
			set { }
		}

		/// <summary>
		/// Tells that the module also creates files.
		/// </summary>
		public virtual bool Creates
		{
			get { return false; }
			set { }
		}
	}

	/// <summary>
	/// Base class of a module editor.
	/// </summary>
	/// <remarks>
	/// This module works with editor events, not with menu commands in editors
	/// (in the latter case use <see cref="ModuleTool"/> configured for editors).
	/// <para>
	/// It is enough to implement <see cref="Invoke"/> method only.
	/// Override other properties and methods as needed.
	/// You may derive any number of such classes.
	/// </para>
	/// <include file='doc.xml' path='docs/pp[@name="InvokeLoad"]/*'/>
	/// </remarks>
	public abstract class ModuleEditor : BaseModule
	{
		/// <summary>
		/// Editor <see cref="IAnyEditor.Opened"/> handler.
		/// </summary>
		/// <remarks>
		/// This method is called once on opening an editor.
		/// Normally you add your editor event handlers, then they do the jobs.
		/// </remarks>
		/// <example>
		/// See the <c>Plugins.NET\TrimSaving</c> module.
		/// It is not just an example, it can be used for real.
		/// </example>
		public abstract void Invoke(object sender, EventArgs e);

		/// <include file='doc.xml' path='docs/pp[@name="FileMask"]/*'/>
		public virtual string Mask
		{
			get { return string.Empty; }
			set { }
		}
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

}
