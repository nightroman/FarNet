/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

using System;
using System.Reflection;

namespace FarManager
{
	/// <summary>
	/// Base class of a FAR.NET plugin.
	/// If a plugin is a single operation then use <see cref="ToolPlugin"/>.
	/// </summary>
	/// <remarks>
	/// It keeps reference to <see cref="IFar"/> and provides
	/// <see cref="Connect"/> and <see cref="Disconnect"/> methods.
	/// Normally a plugin should implement at least <see cref="Connect"/>.
	/// </remarks>
	public class BasePlugin
	{
		IFar _Far;

		/// <summary>
		/// This object exposes all FAR.NET features.
		/// It is set internally and should not be changed directly.
		/// </summary>
		public IFar Far
		{
			get { return _Far; }
			set
			{
				if (_Far != null)
					Disconnect();
				_Far = value;
				if (value != null)
					Connect();
			}
		}

		/// <include file='doc.xml' path='docs/pp[@name="Connect"]/*'/>
		public virtual void Connect() { }

		/// <summary>
		/// Override this to handle plugin shutdown.
		/// CAUTION: don't call FAR UI, if FAR is exiting its UI features do not work.
		/// </summary>
		public virtual void Disconnect() { }

		/// <summary>
		/// Plugin or menu item name. By default it is the class name.
		/// </summary>
		public virtual string Name
		{
			get
			{
				return GetType().FullName;
			}
		}
	}

	/// <summary>
	/// Base class of a FAR.NET tool represented by a single menu command.
	/// It is enough to implement <see cref="Invoke"/> method only.
	/// Override other properties and methods as needed.
	/// </summary>
	public abstract class ToolPlugin : BasePlugin
	{
		/// <summary>
		/// Tool handler.
		/// </summary>
		public abstract void Invoke(object sender, ToolEventArgs e);

		/// <summary>
		/// Tool options. By default the tool is shown in all menus.
		/// Override this to specify only really needed areas.
		/// </summary>
		public virtual ToolOptions Options
		{
			get { return ToolOptions.AllAreas; }
		}
	}

	/// <summary>
	/// Plugin tool options.
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
		/// Show the item in F11 menus.
		/// </summary>
		F11Menus = Panels | Editor | Viewer,
		/// <summary>
		/// Show the item in all menus.
		/// </summary>
		AllMenus = F11Menus | Disk,
		/// <summary>
		/// All areas.
		/// </summary>
		AllAreas = AllMenus | Config
	}

}
