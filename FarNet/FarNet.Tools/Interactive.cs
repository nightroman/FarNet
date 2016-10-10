
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2016 Roman Kuzmin
*/

using System;
using System.IO;
using System.Text;

namespace FarNet.Tools
{
	/// <summary>
	/// TODO
	/// </summary>
	public class InteractiveArea
	{
		/// <summary>
		/// TODO
		/// </summary>
		public int HeadLineIndex { get; set; }
		/// <summary>
		/// TODO
		/// </summary>
		public int LastLineIndex { get; set; }
		/// <summary>
		/// TODO
		/// </summary>
		public Point Caret { get; set; }
		/// <summary>
		/// TODO
		/// </summary>
		public bool Active { get; set; }
	}
	/// <summary>
	/// TODO
	/// </summary>
	public abstract class InteractiveEditor
	{
		/// <summary>
		/// TODO
		/// </summary>
		public IEditor Editor { get; private set; }
		readonly HistoryLog History;
		readonly string OutputMark1;
		readonly string OutputMark2;
		readonly string OutputMark3;
		/// <summary>
		/// TODO
		/// </summary>
		public InteractiveEditor(IEditor editor, HistoryLog history, string outputMark1, string outputMark2, string outputMark3)
		{
			Editor = editor;
			History = history;

			Editor.KeyDown += OnKeyDown;

			OutputMark1 = outputMark1;
			OutputMark2 = outputMark2;
			OutputMark3 = outputMark3;
		}
		/// <summary>
		/// TODO
		/// </summary>
		protected virtual bool IsAsync { get { return false; } }
		/// <summary>
		/// TODO
		/// </summary>
		/// <returns></returns>
		public InteractiveArea GetCommandArea()
		{
			var r = new InteractiveArea();
			r.Caret = Editor.Caret;

			// head line
			for (int y = r.Caret.Y; --y >= 0;)
			{
				var text = Editor[y].Text;
				if (text == OutputMark2 || text == OutputMark3)
				{
					r.HeadLineIndex = y + 1;
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
			while (r.HeadLineIndex < r.LastLineIndex && Editor[r.HeadLineIndex].Length == 0)
				++r.HeadLineIndex;
			while (r.HeadLineIndex < r.LastLineIndex && Editor[r.LastLineIndex].Length == 0)
				--r.LastLineIndex;

			return r;
		}
		/// <summary>
		/// TODO
		/// </summary>
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
			var area = GetCommandArea();
			if (area == null)
				return false;

			// code
			var sb = new StringBuilder();
			for (int y = area.HeadLineIndex; y <= area.LastLineIndex; ++y)
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
			catch (Exception e)
			{
				Editor.InsertText(e.ToString());
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
		void EndOutput()
		{
			if (Editor.Line.Length > 0)
				Editor.InsertLine();
			Editor.InsertText(OutputMark2 + "\r\r");
		}
		/// <summary>
		/// TODO
		/// </summary>
		protected void EndInvoke()
		{
			EndOutput();
			Editor.EndAsync();

			Far.Api.PostJob(delegate
			{
				Editor.Sync();
				Editor.EndUndo();
				Editor.Redraw();
			}
			);
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
		/// TODO
		/// </summary>
		protected virtual bool KeyPressed(KeyInfo key)
		{
			switch (key.VirtualKeyCode)
			{
				case KeyCode.Enter:
					{
						if (key.IsShift())
							return DoInvoke();
						break;
					}
				case KeyCode.Delete:
					{
						if (key.IsShift())
						{
							DoDelete();
							return true;
						}
						break;
					}
				case KeyCode.F6:
					{
						if (key.Is())
						{
							DoHistory();
							return true;
						}
						break;
					}
			}
			return false;
		}
		void OnKeyDown(object sender, KeyEventArgs e)
		{
			// skip selected
			if (Editor.SelectionExists)
				return;

			e.Ignore = KeyPressed(e.Key);
		}
	}
}
