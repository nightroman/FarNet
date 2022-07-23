
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Options to delete temporary source files and empty folders after use.
/// </summary>
public enum DeleteSource
{
	/// <summary>
	/// Default action: do not delete a file.
	/// </summary>
	None,

	/// <summary>
	/// Try to delete a file always. It is not recommended if editor\viewer switching is enabled (F6).
	/// You may set it at any time, i.e. before or after opening.
	/// </summary>
	File,

	/// <summary>
	/// The same as <see cref="File"/> plus delete its folder if it is empty.
	/// You may set it at any time, i.e. before or after opening.
	/// </summary>
	Folder,

	/// <summary>
	/// Delete a file if it was not used. The file is used if:
	/// *) it was saved;
	/// *) there was editor\viewer switching (F6).
	/// *) it is opened in another editor or viewer.
	/// You should set it before opening.
	/// </summary>
	UnusedFile,

	/// <summary>
	/// The same as <see cref="UnusedFile"/> plus delete its folder if it is empty.
	/// You should set it before opening.
	/// </summary>
	UnusedFolder,
}
