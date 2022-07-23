
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Panel view mode.
/// </summary>
public enum PanelViewMode
{
	/// <summary>
	/// Alternative full (Ctrl-0).
	/// </summary>
	AlternativeFull,

	/// <summary>
	/// Brief (Ctrl-1).
	/// </summary>
	Brief,

	/// <summary>
	/// Medium (Ctrl-2).
	/// </summary>
	Medium,

	/// <summary>
	/// Full (Ctrl-3).
	/// </summary>
	Full,

	/// <summary>
	/// Wide (Ctrl-4).
	/// </summary>
	Wide,

	/// <summary>
	/// Detailed (Ctrl-5).
	/// </summary>
	Detailed,

	/// <summary>
	/// Descriptions (Ctrl-6).
	/// </summary>
	Descriptions,

	/// <summary>
	/// LongDescriptions (Ctrl-7).
	/// </summary>
	LongDescriptions,

	/// <summary>
	/// FileOwners (Ctrl-8).
	/// </summary>
	FileOwners,

	/// <summary>
	/// FileLinks (Ctrl-9).
	/// </summary>
	FileLinks,

	/// <summary>
	/// Undefined.
	/// </summary>
	Undefined = -0x30
}
