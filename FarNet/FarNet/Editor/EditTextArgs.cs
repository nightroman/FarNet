
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Arguments of <see cref="IAnyEditor.EditText(EditTextArgs)"/> and <see cref="IAnyEditor.EditTextAsync(EditTextArgs)"/>.
/// </summary>
public class EditTextArgs
{
	/// <summary>
	/// Input text to be edited.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// Editor window title.
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// File extension (for Colorer).
	/// </summary>
	public string Extension { get; set; }

	/// <summary>
	/// Tells to open text locked for changes.
	/// </summary>
	public bool IsLocked { get; set; }

	/// <summary>
	/// Called when the editor is opened.
	/// </summary>
	public EventHandler EditorOpened { get; set; }
}
