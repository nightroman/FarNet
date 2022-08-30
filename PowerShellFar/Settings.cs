
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using FarNet;

namespace PowerShellFar;

/// <summary>
/// PowerShellFar settings, <c>$Psf.Settings</c>
/// </summary>
/// <remarks>
/// The settings are mostly preference variables.
/// They are usually set in the profile.
/// </remarks>
public sealed class Settings
{
	/// <summary>
	/// The default instance. In scripts use <c>$Psf.Settings</c>.
	/// </summary>
	public static Settings Default { get; } = new();

	/// <summary>
	/// Sets the popup list menu template.
	/// </summary>
	/// <param name="menu">The menu with default properties.</param>
	public void PopupMenu(IListMenu menu)
	{
		if (menu is null)
			throw new ArgumentNullException(nameof(menu));

		menu.AutoSelect = PopupAutoSelect;
		menu.MaxHeight = PopupMaxHeight;
		menu.NoShadow = PopupNoShadow;
	}

	/// <summary>
	/// Sets the list menu template.
	/// </summary>
	/// <param name="menu">The menu with default properties.</param>
	public void ListMenu(IListMenu menu)
	{
		if (menu is null)
			throw new ArgumentNullException(nameof(menu));

		menu.ScreenMargin = ListMenuScreenMargin;
		menu.UsualMargins = ListMenuUsualMargins;
	}

	/// <summary>
	/// <see cref="IListMenu.AutoSelect"/> for popup menus.
	/// </summary>
	public bool PopupAutoSelect { get; set; } = true;

	/// <summary>
	/// <see cref="IAnyMenu.MaxHeight"/> for popup menus.
	/// </summary>
	public int PopupMaxHeight { get; set; } = -1;

	/// <summary>
	/// <see cref="IAnyMenu.NoShadow"/> for popup menus.
	/// </summary>
	public bool PopupNoShadow { get; set; }

	/// <summary>
	/// <see cref="IListMenu.ScreenMargin"/> for list menus.
	/// </summary>
	public int ListMenuScreenMargin { get; set; } = 2;

	/// <summary>
	/// <see cref="IListMenu.UsualMargins"/> for list menus.
	/// </summary>
	public bool ListMenuUsualMargins { get; set; } = true;

	/// <summary>
	/// The maximum number of history commands to show.
	/// </summary>
	public int MaximumHistoryCount { get; set; } = 1000;

	int _MaximumPanelColumnCount = 8;

	/// <summary>
	/// The maximum number of columns allowed in free format panels.
	/// </summary>
	public int MaximumPanelColumnCount
	{
		get => _MaximumPanelColumnCount;
		set
		{
			if (value < 3)
				throw new ArgumentException(Res.MaximumPanelColumnCount);

			if (value > FarColumn.DefaultColumnKinds.Count)
				throw new ArgumentException(Res.MaximumPanelColumnCount);

			_MaximumPanelColumnCount = value;
		}
	}

	/// <summary>
	/// The maximum number of files to show before confirmation in some panels.
	/// </summary>
	public int MaximumPanelFileCount { get; set; } = 1000;

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
		get => _FormatEnumerationLimit >= 0 ? _FormatEnumerationLimit : _FormatEnumerationLimit = A.FormatEnumerationLimit;
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
		get => _ExternalViewerFileName;
		set => _ExternalViewerFileName = value ?? throw new ArgumentNullException(nameof(value));
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
		get => _ExternalViewerArguments;
		set => _ExternalViewerArguments = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Gets or sets the command text color.
	/// </summary>
	public ConsoleColor CommandForegroundColor { get; set; } = ConsoleColor.DarkGray;

	/// <summary>
	/// Gets or sets the debug text color.
	/// </summary>
	public ConsoleColor DebugForegroundColor { get; set; } = ConsoleColor.Magenta;

	/// <summary>
	/// Gets or sets the error text color.
	/// </summary>
	public ConsoleColor ErrorForegroundColor { get; set; } = ConsoleColor.Red;

	/// <summary>
	/// Gets or sets the verbose text color.
	/// </summary>
	public ConsoleColor VerboseForegroundColor { get; set; } = ConsoleColor.Yellow;

	/// <summary>
	/// Gets or sets the warning text color.
	/// </summary>
	public ConsoleColor WarningForegroundColor { get; set; } = ConsoleColor.Yellow;

	/// <summary>
	/// Tells to remove console output ANSI rendering.
	/// Default: false for Windows 10+, true otherwise.
	/// </summary>
	public bool RemoveOutputRendering { get; set; } = Environment.OSVersion.Version.Major < 10;
}
