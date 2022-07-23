
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Far Manager settings.
/// </summary>
public enum FarSetting
{
	///
	None,

	/// <summary>
	/// Confirmation settings:
	/// Copy, Move, RO, Drag, Delete, DeleteFolder, Esc, HistoryClear, Exit, RemoveConnection.
	/// </summary>
	Confirmations = 16,

	/// <summary>
	/// System settings:
	/// DeleteToRecycleBin, CopyOpened, PluginMaxReadData, ScanJunction.
	/// </summary>
	System = 17,

	/// <summary>
	/// Panels settings:
	/// ShowHidden.
	/// </summary>
	Panels = 18,

	/// <summary>
	/// Editor settings:
	/// WordDiv.
	/// </summary>
	Editor = 19,

	/// <summary>
	/// Screen settings:
	/// KeyBar.
	/// </summary>
	Screen = 20,

	/// <summary>
	/// Dialog settings:
	/// EditBlock, EULBsClear, DelRemovesBlocks.
	/// </summary>
	Dialog = 21,

	/// <summary>
	/// Interface settings:
	/// ShowMenuBar.
	/// </summary>
	Interface = 22,

	/// <summary>
	/// Panel layout settings:
	/// ColumnTitles, StatusLine, SortMode.
	/// </summary>
	PanelLayout = 23,
}
