
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// PowerShellFar settings. Exposed as <c>$Psf.Settings</c>
	/// </summary>
	/// <remarks>
	/// The settings are mostly preference variables.
	/// They are usually set in the profile.
	/// </remarks>
	public sealed class Settings
	{
		/// <summary>
		/// The settings object exposed as <c>$Psf.Settings</c>.
		/// </summary>
		public static Settings Default { get { return _Default; } }
		static readonly Settings _Default = new Settings();
		/// <summary>
		/// Sets the popup list menu template.
		/// </summary>
		/// <param name="menu">The menu with default properties.</param>
		public void PopupMenu(IListMenu menu)
		{
			if (menu == null) throw new ArgumentNullException("menu");
			menu.AutoSelect = _PopupAutoSelect;
			menu.MaxHeight = _PopupMaxHeight;
			menu.NoShadow = _PopupNoShadow;
		}
		/// <summary>
		/// Sets the list menu template.
		/// </summary>
		/// <param name="menu">The menu with default properties.</param>
		public void ListMenu(IListMenu menu)
		{
			if (menu == null) throw new ArgumentNullException("menu");
			menu.ScreenMargin = _ListMenuScreenMargin;
			menu.UsualMargins = _ListMenuUsualMargins;
		}
		bool _PopupAutoSelect = true;
		/// <summary>
		/// <see cref="IListMenu.AutoSelect"/> for popup menus.
		/// </summary>
		public bool PopupAutoSelect
		{
			get { return _PopupAutoSelect; }
			set { _PopupAutoSelect = value; }
		}
		int _PopupMaxHeight = -1;
		/// <summary>
		/// <see cref="IAnyMenu.MaxHeight"/> for popup menus.
		/// </summary>
		public int PopupMaxHeight
		{
			get { return _PopupMaxHeight; }
			set { _PopupMaxHeight = value; }
		}
		bool _PopupNoShadow;
		/// <summary>
		/// <see cref="IListMenu.NoShadow"/> for popup menus.
		/// </summary>
		public bool PopupNoShadow
		{
			get { return _PopupNoShadow; }
			set { _PopupNoShadow = value; }
		}
		int _ListMenuScreenMargin = 2;
		/// <summary>
		/// <see cref="IListMenu.ScreenMargin"/> for list menus.
		/// </summary>
		public int ListMenuScreenMargin
		{
			get { return _ListMenuScreenMargin; }
			set { _ListMenuScreenMargin = value; }
		}
		bool _ListMenuUsualMargins = true;
		/// <summary>
		/// <see cref="IListMenu.UsualMargins"/> for list menus.
		/// </summary>
		public bool ListMenuUsualMargins
		{
			get { return _ListMenuUsualMargins; }
			set { _ListMenuUsualMargins = value; }
		}
		int _MaximumHistoryCount = 512;
		/// <summary>
		/// The maximum number of history commands kept in a file. In fact, 10% more is allowed.
		/// </summary>
		public int MaximumHistoryCount
		{
			get { return _MaximumHistoryCount; }
			set { _MaximumHistoryCount = value; }
		}
		int _MaximumPanelColumnCount = 8;
		/// <summary>
		/// The maximum number of columns allowed in free format panels.
		/// </summary>
		public int MaximumPanelColumnCount
		{
			get { return _MaximumPanelColumnCount; }
			set
			{
				if (value < 3) throw new ArgumentException(Res.MaximumPanelColumnCount);
				if (value > FarColumn.DefaultColumnKinds.Count) throw new ArgumentException(Res.MaximumPanelColumnCount);
				_MaximumPanelColumnCount = value;
			}
		}
		int _MaximumPanelFileCount = 1000;
		/// <summary>
		/// The maximum number of files to show before confirmation in some panels.
		/// </summary>
		public int MaximumPanelFileCount
		{
			get { return _MaximumPanelFileCount; }
			set { _MaximumPanelFileCount = value; }
		}
		int _FormatEnumerationLimit = -1;
		/// <summary>
		/// Determines how many enumerated items are included in a display.
		/// </summary>
		/// <remarks>
		/// The default is the $FormatEnumerationLimit value, usually 4.
		/// <para>
		/// See PowerShell help about_Preference_Variables, $FormatEnumerationLimit.
		/// </para>
		/// </remarks>
		public int FormatEnumerationLimit
		{
			get
			{
				return _FormatEnumerationLimit >= 0 ? _FormatEnumerationLimit : _FormatEnumerationLimit = A.FormatEnumerationLimit;
			}
			set
			{
				if (value >= 0)
					_FormatEnumerationLimit = value;
			}
		}
		string _ExternalViewerFileName = string.Empty;
		/// <summary>
		/// The external viewer application path.
		/// </summary>
		/// <remarks>
		/// By default it is empty and external Far viewer is used.
		/// <example>
		/// See <see cref="ExternalViewerArguments"/>
		/// </example>
		/// </remarks>
		public string ExternalViewerFileName
		{
			get { return _ExternalViewerFileName; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				_ExternalViewerFileName = value;
			}
		}
		string _ExternalViewerArguments = string.Empty;
		/// <summary>
		/// The command line arguments for the external viewer.
		/// </summary>
		/// <remarks>
		/// It is used together with <see cref="ExternalViewerFileName"/>.
		/// Use "{0}" where a file path should be inserted.
		/// <example>
		/// <code>
		/// $Psf.Settings.ExternalViewerFileName = "$env:FARHOME\Far.exe"
		/// $Psf.Settings.ExternalViewerArguments = '/m /p /v "{0}"'
		/// </code>
		/// </example>
		/// </remarks>
		public string ExternalViewerArguments
		{
			get { return _ExternalViewerArguments; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				_ExternalViewerArguments = value;
			}
		}
		ConsoleColor _CommandForegroundColor = ConsoleColor.DarkGray;
		/// <summary>
		/// Gets or sets the command text color.
		/// </summary>
		public ConsoleColor CommandForegroundColor
		{
			get { return _CommandForegroundColor; }
			set { _CommandForegroundColor = value; }
		}
		ConsoleColor _DebugForegroundColor = ConsoleColor.Magenta;
		/// <summary>
		/// Gets or sets the debug text color.
		/// </summary>
		public ConsoleColor DebugForegroundColor
		{
			get { return _DebugForegroundColor; }
			set { _DebugForegroundColor = value; }
		}
		ConsoleColor _ErrorForegroundColor = ConsoleColor.Red;
		/// <summary>
		/// Gets or sets the error text color.
		/// </summary>
		public ConsoleColor ErrorForegroundColor
		{
			get { return _ErrorForegroundColor; }
			set { _ErrorForegroundColor = value; }
		}
		ConsoleColor _VerboseForegroundColor = ConsoleColor.Cyan;
		/// <summary>
		/// Gets or sets the verbose text color.
		/// </summary>
		public ConsoleColor VerboseForegroundColor
		{
			get { return _VerboseForegroundColor; }
			set { _VerboseForegroundColor = value; }
		}
		ConsoleColor _WarningForegroundColor = ConsoleColor.Yellow;
		/// <summary>
		/// Gets or sets the warning text color.
		/// </summary>
		public ConsoleColor WarningForegroundColor
		{
			get { return _WarningForegroundColor; }
			set { _WarningForegroundColor = value; }
		}
		/// <summary>
		/// The script invoked after editor console commands.
		/// </summary>
		/// <remarks>
		/// The script is invoked after each editor console command, its output
		/// is converted to strings and written to the editor console.
		/// </remarks>
		/// <example><code>
		/// # In the StartupCode
		/// $Psf.Settings.EditorConsoleEndOutputScript = 'Get-Date'
		/// </code></example>
		public String EditorConsoleEndOutputScript { get; set; }
	}
}
