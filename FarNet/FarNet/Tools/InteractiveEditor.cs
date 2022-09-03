
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.IO;
using System.Text;

namespace FarNet.Tools;

/// <summary>
/// Base interactive editor used by PowerShellFar and FSharpFar interactives.
/// </summary>
public abstract class InteractiveEditor
{
	/// <summary>
	/// The connected editor.
	/// </summary>
	public IEditor Editor { get; }

	readonly HistoryLog History;
	readonly string OutputMark1;
	readonly string OutputMark2;
	readonly string OutputMark3;
	HistoryNext Next;

	/// <summary>
	/// New instance.
	/// </summary>
	/// <param name="editor">The connected editor.</param>
	/// <param name="history">The connected history log.</param>
	/// <param name="outputMark1">The opening output mark.</param>
	/// <param name="outputMark2">The closing output mark.</param>
	/// <param name="outputMark3">The empty output mark.</param>
	protected InteractiveEditor(IEditor editor, HistoryLog history, string outputMark1, string outputMark2, string outputMark3)
	{
		Editor = editor;
		History = history;

		OutputMark1 = outputMark1;
		OutputMark2 = outputMark2;
		OutputMark3 = outputMark3;

		Editor.KeyDown += Editor_KeyDown;
		Editor.Changed += Editor_Changed;
	}

	/// <summary>
	/// Tells to save after output.
	/// </summary>
	public bool AutoSave { get; set; }

	/// <summary>
	/// Gets true if the editor is async.
	/// </summary>
	protected virtual bool IsAsync => false;

	/// <summary>
	/// Gets the current interactive area where the caret is.
	/// </summary>
	public InteractiveArea CommandArea()
	{
		var r = new InteractiveArea
		{
			Caret = Editor.Caret
		};

		// head line
		for (int y = r.Caret.Y; --y >= 0;)
		{
			var text = Editor[y].Text;
			if (text == OutputMark2 || text == OutputMark3)
			{
				r.FirstLineIndex = y + 1;
				break;
			}

			if (text == OutputMark1)
				return null;
		}

		r.Active = true;

		// last line
		r.LastLineIndex = Editor.Count - 1;
		for (int y = r.Caret.Y; ++y <= r.LastLineIndex;)
		{
			var text = Editor[y].Text;
			if (text == OutputMark1 || text == OutputMark3)
			{
				r.LastLineIndex = y - 1;
				r.Active = false;
				break;
			}

			if (text == OutputMark2)
				return null;
		}

		// trim
		while (r.FirstLineIndex < r.LastLineIndex && Editor[r.FirstLineIndex].Length == 0)
			++r.FirstLineIndex;
		while (r.FirstLineIndex < r.LastLineIndex && Editor[r.LastLineIndex].Length == 0)
			--r.LastLineIndex;

		return r;
	}

	/// <summary>
	/// Invokes the current code.
	/// </summary>
	/// <param name="code">Current code to be invoked.</param>
	/// <param name="area">Current interactive editor area info.</param>
	protected abstract void Invoke(string code, InteractiveArea area);

	void DoHistory()
	{
		var ui = new HistoryMenu(History);
		var code = ui.Show();
		if (code == null)
			return;

		code = code.Replace(OutputMark3, Environment.NewLine);

		Editor.BeginUndo();
		Editor.GoToEnd(true);
		Editor.InsertText(code);
		Editor.EndUndo();
		Editor.Redraw();
	}

	bool DoInvoke()
	{
		var area = CommandArea();
		if (area == null)
			return false;

		// code
		var sb = new StringBuilder();
		for (int y = area.FirstLineIndex; y <= area.LastLineIndex; ++y)
		{
			if (sb.Length > 0)
				sb.AppendLine();
			sb.Append(Editor[y].Text);
		}

		// skip empty
		string code = sb.ToString();
		if (code.Length == 0)
			return true;
		if (code == OutputMark3)
			return false;

		// copy to the end
		if (!area.Active)
		{
			Editor.BeginUndo();
			Editor.GoToEnd(true);
			Editor.InsertText(code);
			Editor.EndUndo();
			Editor.Redraw();
			return true;
		}

		// history
		History.AddLine(code.Replace(Environment.NewLine, OutputMark3));

		// begin
		Editor.BeginUndo();
		Editor.GoToEnd(false);
		if (Editor.Line.Length > 0)
			Editor.InsertLine();
		Editor.InsertText(OutputMark1 + "\r");

		try
		{
			if (IsAsync)
				Editor.BeginAsync();

			Invoke(code, area);
		}
		catch (ModuleException ex)
		{
			Editor.InsertText($"{ex.Source}: {ex.Message}");
		}
		catch (Exception ex)
		{
			Editor.InsertText(ex.ToString());
		}
		finally
		{
			//! may be closed :: #quit in F#
			if (!IsAsync && Editor.IsOpened)
			{
				Editor.GoToEnd(false);
				EndOutput();
				Editor.EndUndo();
				Editor.Redraw();
			}
		}
		return true;
	}

	bool DoNext(bool up)
	{
		// for navigation it should be a single line command area with the caret at the last editor line
		var area = CommandArea();
		if (area == null || area.FirstLineIndex != area.LastLineIndex || area.Caret.Y != Editor.Count - 1)
		{
			Next = null;
			return false;
		}

		// start or continue navigation
		Next ??= new(History.ReadLines(), Editor.Line.Text);
		while (true)
		{
			// find the next simple line
			var line = Next.GetNext(up, Editor.Line.Text);
			if (line.Contains(OutputMark3))
				continue;

			// set the current line (keep/restore Next, it is nulled on changes)
			var next = Next;
			Editor.Line.Text = line;
			Editor.Line.Caret = -1;
			Editor.Redraw();
			Next = next;
			break;
		}

		return true;
	}

	void EndOutput()
	{
		if (Editor.IsOpened)
		{
			if (Editor.Line.Length > 0)
				Editor.InsertLine();

			Editor.InsertText(OutputMark2 + "\r\r");

			if (AutoSave)
				Editor.Save();
		}
	}

	/// <summary>
	/// It is called in the end of <see cref="Invoke"/>.
	/// </summary>
	protected void EndInvoke()
	{
		EndOutput();
		Editor.EndAsync();

		Far.Api.PostJob(() =>
		{
			Editor.Sync();
			Editor.EndUndo();
			Editor.Redraw();
		});
	}

	void DoDelete()
	{
		var caret = Editor.Caret;

		int line1 = -1;
		for (int i = caret.Y; i >= 0; --i)
		{
			if (Editor[i].Text == OutputMark1)
			{
				line1 = i;
				break;
			}
		}
		if (line1 < 0)
			return;

		int line2 = -1;
		int n = Editor.Count;
		for (int i = line1; i < n; ++i)
		{
			if (Editor[i].Text == OutputMark2)
			{
				line2 = i;
				break;
			}
		}
		if (line2 < 0)
			return;

		Editor.BeginUndo();
		Editor.SelectText(0, line1, OutputMark2.Length, line2, PlaceKind.Stream);
		Editor.DeleteText();
		Editor.GoTo(0, line1);
		Editor.InsertText(OutputMark3 + "\r");
		Editor.GoTo(0, line1);
		Editor.EndUndo();

		Editor.Redraw();
	}

	/// <summary>
	/// Handles keys.
	/// The default processes ShiftEnter, ShiftDel, F5, Up/Down (last line).
	/// </summary>
	/// <param name="key">Key info.</param>
	/// <returns>True if the key was processed and will be ignored.</returns>
	/// <remarks>
	/// This method is not called when any selection exists.
	/// In this case the interactive works as normal editor.
	/// </remarks>
	protected virtual bool KeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.Enter when key.IsShift():
				Next = null;
				return DoInvoke();

			case KeyCode.Delete when key.IsShift():
				Next = null;
				DoDelete();
				return true;

			case KeyCode.F5 when key.Is():
				Next = null;
				DoHistory();
				return true;

			case KeyCode.UpArrow when key.Is():
				return DoNext(true);

			case KeyCode.DownArrow when key.Is():
				return DoNext(false);
		}

		return false;
	}

	// see KeyPressed remarks about selection
	void Editor_KeyDown(object sender, KeyEventArgs e)
	{
		if (!Editor.SelectionExists)
			e.Ignore = KeyPressed(e.Key);
	}

	// Reset history navigation on changes. If we have started editing the
	// command and accidentally hit [Up] then [Down] restores the changes.
	void Editor_Changed(object sender, EditorChangedEventArgs e)
	{
		Next = null;
	}
}
