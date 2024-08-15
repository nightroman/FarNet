
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FarNet.Works;
#pragma warning disable 1591

public static class EditorTools
{
	public static IEnumerable<ILine> EnumerateLines(IEditor editor, int start, int end)
	{
		for (int i = start; i < end; ++i)
			yield return editor[i];
	}

	public static IEnumerable<string> EnumerateStrings(IEditor editor, int start, int end)
	{
		for (int i = start; i < end; ++i)
			yield return editor[i].Text;
	}

	// Creates an editor. Async safe. Unlikely throws.
	static IEditor EditTextCreate(EditTextArgs args)
	{
		//! FarNet API, safe for async
		var editor = Far.Api.CreateEditor();

		//! avoid native API, do not use TempName
		editor.FileName = Kit.TempFileName(args.Extension);
		editor.CodePage = 65001;
		editor.DisableHistory = true;
		if (!string.IsNullOrEmpty(args.Title))
			editor.Title = args.Title;
		if (args.IsLocked)
			editor.IsLocked = true;
		if (args.EditorOpened is { })
			editor.Opened += args.EditorOpened;
		if (args.EditorSaving is { })
			editor.Saving += args.EditorSaving;

		//? unlikely throws
		if (!string.IsNullOrEmpty(args.Text))
			File.WriteAllText(editor.FileName, args.Text);

		return editor;
	}

	// Gets the edit text result. May throw.
	static string? EditTextResult(IEditor editor, EditTextArgs args)
	{
		try
		{
			if (args.IsLocked)
				return null;

			if (File.Exists(editor.FileName))
			{
				// read and return
				return File.ReadAllText(editor.FileName);
			}
			else
			{
				// no file, e.g. the text was empty and user exits without saving; case 080502
				return string.Empty;
			}
		}
		finally
		{
			try
			{
				File.Delete(editor.FileName);
			}
			catch (IOException e)
			{
				Log.TraceException(e);
			}
		}
	}

	public static string? EditText(EditTextArgs args)
	{
		ArgumentNullException.ThrowIfNull(args);

		var editor = EditTextCreate(args);
		editor.Open(OpenMode.Modal);
		return EditTextResult(editor, args);
	}

	public static async Task<string?> EditTextAsync(EditTextArgs args)
	{
		ArgumentNullException.ThrowIfNull(args);

		var editor = EditTextCreate(args);
		await Tasks.Editor(editor);
		return EditTextResult(editor, args);
	}

	/*
	Issue [_090219_121638] On switching to editor the temp file is not deleted;
	?? Editor in READ event can check existing viewer with DeleteSource::File,
	drop the flag for editor and propagate this option to itself.
	*/
	public static void ViewText(string text, string title, OpenMode mode)
	{
		string tmpfile = Far.Api.TempName();
		File.WriteAllText(tmpfile, text);

		var viewer = Far.Api.CreateViewer();
		viewer.FileName = tmpfile;
		viewer.CodePage = 65001;
		viewer.DeleteSource = DeleteSource.File; // yes, File - we can control it
		viewer.DisableHistory = true;
		viewer.Switching = Switching.Enabled;
		viewer.Title = title;
		viewer.Open(mode);
	}

	const string MenuHotkeys = "0123456789abcdefghijklmnopqrstuvwxyz";
	const string MenuItemFormat = "&{0}. {1}";

	public static void ShowEditorsMenu()
	{
		var editors = Far.Api.Editors();

		int head = Far.Api.Window.Kind == WindowKind.Editor ? 1 : 0;
		int tail = editors.Length - 1;

		if (head > tail)
			return;

		if (head == tail)
		{
			editors[head].Activate();
			return;
		}

		var menu = Far.Api.CreateMenu();
		menu.HelpTopic = "editors-menu";
		menu.Title = "Editors";

		int index = -1;
		for (int i = head; i <= tail; ++i)
		{
			++index;
			var editor = editors[i];
			var name = string.Format(
				MenuItemFormat,
				index < MenuHotkeys.Length ? MenuHotkeys.Substring(index, 1) : " ",
				editor.Title);
			menu.Add(name).Data = editor;
		}

		if (menu.Show())
			((IEditor)menu.SelectedData!).Activate();
	}

	public static void ShowViewersMenu()
	{
		var menu = Far.Api.CreateMenu();
		menu.HelpTopic = "viewers-menu";
		menu.Title = "Viewers";

		int index = -1;
		foreach (var it in Far.Api.Viewers())
		{
			++index;
			var name = string.Format(
				MenuItemFormat,
				index < MenuHotkeys.Length ? MenuHotkeys.Substring(index, 1) : " ",
				it.FileName);
			menu.Add(name).Data = it;
		}

		if (menu.Show())
			((IViewer)menu.SelectedData!).Activate();
	}
}
