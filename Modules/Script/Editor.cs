
// Escapes and unescapes \ and " in editor selection with \
// This module implements two items shown in the editor plugin menu.

using FarNet;
using System;
using System.Text.RegularExpressions;

namespace Script;

public class Editor
{
	readonly IEditor _editor;

	public Editor()
	{
		_editor = Far.Api.Editor ?? throw new InvalidOperationException("This operation requires an editor.");
	}

	// Escapes \ and " in editor selection with \
	public void Escape()
	{
		if (_editor.SelectionExists)
			_editor.SetSelectedText(Regex.Replace(_editor.GetSelectedText("\r"), @"([\\""])", @"\$1"));
	}

	// Unescapes \ and " in editor selection with \
	public void Unescape()
	{
		if (_editor.SelectionExists)
			_editor.SetSelectedText(Regex.Replace(_editor.GetSelectedText("\r"), @"\\([\\""])", "$1"));
	}
}
