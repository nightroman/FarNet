
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Export file arguments.
/// </summary>
/// <param name="mode">See <see cref="ExplorerEventArgs.Mode"/></param>
/// <param name="file">See <see cref="ExplorerFileEventArgs.File"/></param>
/// <param name="fileName">See <see cref="FileName"/></param>
public class GetContentEventArgs(ExplorerModes mode, FarFile file, string fileName) : ExplorerFileEventArgs(mode, file)
{
	/// <summary>
	/// Gets the destination file path.
	/// </summary>
	public string FileName { get; } = fileName;

	/// <summary>
	/// Tells that the file can be updated.
	/// </summary>
	/// <remarks>
	/// Use case. The core opens the file in the editor. By default the editor is locked:
	/// the core assumes the changes will be lost. This flag tells to not lock the editor.
	/// </remarks>
	public bool CanSet { get; set; }

	/// <summary>
	/// Gets or set the exported text.
	/// </summary>
	/// <remarks>
	/// It can be a string or an object to be converted by <c>ToString</c>
	/// or a collection of objects to be converted to lines by <c>ToString</c>.
	/// </remarks>
	public object? UseText { get; set; }

	/// <summary>
	/// Gets or set the actual source file name to be used instead.
	/// </summary>
	public string? UseFileName { get; set; }

	/// <summary>
	/// Gets or sets the file extension to use.
	/// </summary>
	/// <remarks>
	/// It is used on opening the file in the editor.
	/// The extension may be useful in order to get proper syntax highlighting with the <i>Colorer</i> plugin.
	/// </remarks>
	public string? UseFileExtension { get; set; }

	/// <summary>
	/// Gets or sets the code page to use in the editor.
	/// </summary>
	/// <remarks>
	/// It may be used together with <see cref="UseFileName"/>.
	/// </remarks>
	public int CodePage { get; set; }

	/// <summary>
	/// Called when the editor is opened.
	/// </summary>
	public EventHandler? EditorOpened { get; set; }
}
