
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using FarNet.Works;
using System.IO;

namespace JavaScriptFar;

[System.Runtime.InteropServices.Guid("895b09e1-b286-4ee7-8121-14c6825a2223")]
[ModuleEditor(Name = "Editor", Mask = "*.js;*.cjs;*.mjs")]
public class JavaScriptEditor : ModuleEditor
{
	IEditor _editor;

	public override void Invoke(IEditor editor, ModuleEditorEventArgs e)
	{
		_editor = editor;
		editor.KeyDown += Editor_KeyDown;
	}

	private void Editor_KeyDown(object sender, KeyEventArgs e)
	{
		switch (e.Key.VirtualKeyCode)
		{
			case KeyCode.F5 when e.Key.Is():
				Execute();
				break;
		}
	}

	private void Execute()
	{
		_editor.Save();

		string print = string.Empty;
		try
		{
			Actor.Execute(new ExecuteArgs { Document = _editor.FileName, Print = s => print = s });
		}
		finally
		{
			if (print.Length > 0)
			{
				if (print.Contains('\n'))
				{
					//! now set the title, or a new editor shows this title until redraw
					_editor.Title = _editor.FileName;

					//! then open a new editor
					var temp = Kit.TempFileName("txt");
					File.WriteAllText(temp, print);
					ShowTempFile(temp, Res.Result);
				}
				else
				{
					_editor.Title = print + " -- " + _editor.FileName;
				}
			}
			else
			{
				_editor.Title = _editor.FileName;
			}
		}
	}

	private static void ShowTempFile(string fileName, string title)
	{
		var editor = Far.Api.CreateEditor();
		editor.Title = title;
		editor.FileName = fileName;
		editor.CodePage = 65001;
		editor.IsLocked = true;
		editor.DisableHistory = true;
		editor.DeleteSource = DeleteSource.UnusedFile;
		editor.Open();
	}
}
