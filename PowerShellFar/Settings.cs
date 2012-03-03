
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System;
using System.Configuration;
using FarNet;
using FarNet.Settings;

namespace PowerShellFar
{
	/// <summary>
	/// PowerShellFar settings. Exposed as <c>$Psf.Settings</c>
	/// </summary>
	/// <remarks>
	/// Properties <see cref="StartupCode"/> and <see cref="StartupEdit"/> are stored
	/// in a file and can be changed in the module settings panel.
	/// <para>
	/// Other properties are session preferences and normally set in the profile.
	/// </para>
	/// <example>Profile-.ps1</example>
	/// </remarks>
	[SettingsProvider(typeof(ModuleSettingsProvider))]
	public sealed class Settings : ModuleSettings
	{
		/// <summary>
		/// The settings exposed as <c>$Psf.Settings</c>.
		/// </summary>
		public static Settings Default { get { return _Default; } }
		static readonly Settings _Default = new Settings();
		/// <summary>
		/// Sets popup list menu.
		/// </summary>
		public void PopupMenu(IListMenu menu)
		{
			if (menu == null) throw new ArgumentNullException("menu");
			menu.AutoSelect = _PopupAutoSelect;
			menu.MaxHeight = _PopupMaxHeight;
			menu.NoShadow = _PopupNoShadow;
		}
		/// <summary>
		/// Sets list menu.
		/// </summary>
		public void ListMenu(IListMenu menu)
		{
			if (menu == null) throw new ArgumentNullException("menu");
			menu.ScreenMargin = _ListMenuScreenMargin;
			menu.UsualMargins = _ListMenuUsualMargins;
		}
		/// <summary>
		/// See the manual [Settings].
		/// </summary>
		[UserScopedSetting]
		[SettingsManageability(SettingsManageability.Roaming)]
		public string StartupCode
		{
			get { return (string)this["StartupCode"]; }
			set { this["StartupCode"] = value; }
		}
		/// <summary>
		/// See the manual [Settings].
		/// </summary>
		[UserScopedSetting]
		[SettingsManageability(SettingsManageability.Roaming)]
		public string StartupEdit
		{
			get { return (string)this["StartupEdit"]; }
			set { this["StartupEdit"] = value; }
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
		///
		public ConsoleColor CommandForegroundColor
		{
			get { return _CommandForegroundColor; }
			set { _CommandForegroundColor = value; }
		}
		ConsoleColor _DebugForegroundColor = ConsoleColor.Magenta;
		///
		public ConsoleColor DebugForegroundColor
		{
			get { return _DebugForegroundColor; }
			set { _DebugForegroundColor = value; }
		}
		ConsoleColor _ErrorForegroundColor = ConsoleColor.Red;
		///
		public ConsoleColor ErrorForegroundColor
		{
			get { return _ErrorForegroundColor; }
			set { _ErrorForegroundColor = value; }
		}
		ConsoleColor _VerboseForegroundColor = ConsoleColor.Cyan;
		///
		public ConsoleColor VerboseForegroundColor
		{
			get { return _VerboseForegroundColor; }
			set { _VerboseForegroundColor = value; }
		}
		ConsoleColor _WarningForegroundColor = ConsoleColor.Yellow;
		///
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
