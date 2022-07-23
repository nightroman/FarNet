
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Open modes of editor and viewer.
/// </summary>
public enum OpenMode
{
	/// <summary>
	/// Tells to open not modal editor or viewer and return immediately.
	/// </summary>
	None,

	/// <summary>
	/// Tells to open not modal editor or viewer and wait for exit.
	/// </summary>
	Wait,

	/// <summary>
	/// Tells to open modal editor or viewer.
	/// </summary>
	Modal,
}
