using FarNet;
using System;
using System.Text.RegularExpressions;

namespace Script;

// Type with instance fn-methods.
public class Editor
{
	readonly IEditor _editor;

	// The constructor sets the common scene for all methods.
	public Editor()
	{
		_editor = Far.Api.Editor ?? throw new InvalidOperationException("This operation requires an editor.");
	}

	// Escapes \ and " with \ in editor selection
	// fn: script=Script; method=Script.Editor.Escape
	public void Escape()
	{
		if (_editor.SelectionExists)
			_editor.SetSelectedText(Regex.Replace(_editor.GetSelectedText("\r"), @"([\\""])", @"\$1"));
	}

	// Unescapes \\ and \" in editor selection
	// fn: script=Script; method=Script.Editor.Unescape
	public void Unescape()
	{
		if (_editor.SelectionExists)
			_editor.SetSelectedText(Regex.Replace(_editor.GetSelectedText("\r"), @"\\([\\""])", "$1"));
	}
}
