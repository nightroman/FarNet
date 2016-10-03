
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2016 Roman Kuzmin
*/

using System;
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
		readonly string OutputMark1;
		readonly string OutputMark2;
		readonly string OutputMark3;

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="editor"></param>
		/// <param name="outputMark1"></param>
		/// <param name="outputMark2"></param>
		/// <param name="outputMark3"></param>
		public InteractiveEditor(IEditor editor, string outputMark1, string outputMark2, string outputMark3)
		{
			Editor = editor;
			Editor.KeyDown += OnKeyDown;

			OutputMark1 = outputMark1;
			OutputMark2 = outputMark2;
			OutputMark3 = outputMark3;
		}

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

		/// <summary>
		/// TODO
		/// </summary>
		protected virtual bool IsAsync { get { return false; } }

		bool DoInvoke()
		{
			var area = GetCommandArea();
			if (area == null)
				return false;

			// script, skip empty
			var sb = new StringBuilder();
			for (int y = area.HeadLineIndex; y < area.LastLineIndex; ++y)
				sb.AppendLine(Editor[y].Text);
			var lastText = Editor[area.LastLineIndex].Text;
			sb.Append(lastText);

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

		/// <summary>
		/// TODO
		/// </summary>
		protected bool IsLastLineCurrent
		{
			get
			{
				return Editor.Caret.Y == Editor.Count - 1;
			}
		}

		void DoDelete()
		{
			if (!IsLastLineCurrent)
				return;

			if (Editor.Line.Length > 0)
				return;

			Point pt = Editor.Caret;
			for (int i = pt.Y - 1; i >= 0; --i)
			{
				string text = Editor[i].Text;
				if (text == OutputMark1)
				{
					Editor.SelectText(0, i, -1, pt.Y, PlaceKind.Stream);
					Editor.DeleteText();
					Editor.GoTo(0, i);
					Editor.InsertText(OutputMark3 + "\r");
					Editor.GoToEnd(false);
					break;
				}
				if (text == OutputMark2 || text == OutputMark3)
				{
					pt = new Point(-1, i + 1);
					continue;
				}
			}

			Editor.Redraw();
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
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
