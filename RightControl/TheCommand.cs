
/*
FarNet module RightControl
Copyright (c) 2010-2014 Roman Kuzmin
*/

using System;
using System.Text.RegularExpressions;
using FarNet.Forms;

namespace FarNet.RightControl
{
	[System.Runtime.InteropServices.Guid("1b42c03e-40c4-45db-a3ce-eb0825fe16d1")]
	[ModuleCommand(Name = TheCommand.Name, Prefix = TheCommand.Name)]
	public class TheCommand : ModuleCommand
	{
		const string Name = "RightControl";
		static Regex Regex { get { if (_Regex_ == null) InitRegex(null); return _Regex_; } }
		static Regex _Regex_;
		public override void Invoke(object sender, ModuleCommandEventArgs e)
		{
			InitRegex(Manager);

			ILine line = null;
			IEditor editor = null;
			var kind = Far.Api.Window.Kind;
			if (kind == WindowKind.Editor)
			{
				editor = Far.Api.Editor;
			}
			else
			{
				line = Far.Api.Line;
				if (line == null)
					return;
			}

			switch (e.Command.Trim())
			{
				case "step-left": Run(editor, line, Operation.Step, false, false); break;
				case "step-right": Run(editor, line, Operation.Step, true, false); break;
				case "select-left": Run(editor, line, Operation.Select, false, false); break;
				case "select-right": Run(editor, line, Operation.Select, true, false); break;
				case "delete-left": Run(editor, line, Operation.Delete, false, false); break;
				case "delete-right": Run(editor, line, Operation.Delete, true, false); break;
				case "vertical-left": if (editor == null) SelectWorkaround(line, false); else Run(editor, line, Operation.Select, false, true); break;
				case "vertical-right": if (editor == null) SelectWorkaround(line, true); else Run(editor, line, Operation.Select, true, true); break;
				case "go-to-smart-home": Home(editor, line, false); break;
				case "select-to-smart-home": Home(editor, line, true); break;
				default: throw new ModuleException("Unknown command: " + e.Command);
			}
		}
		static void InitRegex(IModuleManager manager)
		{
			if (_Regex_ != null)
				return;

			var settings = new Settings();
			try
			{
				_Regex_ = new Regex(settings.Regex, RegexOptions.IgnorePatternWhitespace);
			}
			catch (Exception e)
			{
				Far.Api.Message("Regular expression error:\r" + e.Message, "RightControl", MessageOptions.LeftAligned | MessageOptions.Warning);
				_Regex_ = new Regex(Settings.RegexDefault, RegexOptions.IgnorePatternWhitespace);
			}
		}
		/// <summary>
		/// Operation kind.
		/// </summary>
		enum Operation
		{
			Step,
			Select,
			Delete
		}
		/// <summary>
		/// Runs the specified operation.
		/// </summary>
		/// <param name="editor">The active editor or null.</param>
		/// <param name="line">The active line or null.</param>
		/// <param name="operation">The operation to run.</param>
		/// <param name="right">True for right, false for left.</param>
		/// <param name="alt">True for the alternative operation.</param>
		static void Run(IEditor editor, ILine line, Operation operation, bool right, bool alt)
		{
			Point caret = line == null ? editor.Caret : new Point(line.Caret, 0);
			int iColumn = caret.X;
			int iLine = caret.Y;

			for (; ; )
			{
				ILine currentLine = line ?? editor[iLine];
				var text = currentLine.Text;

				int newX = -1;
				foreach (Match match in Regex.Matches(text))
				{
					if (right)
					{
						if (match.Index > iColumn)
						{
							newX = match.Index;
							break;
						}
					}
					else
					{
						if (match.Index >= iColumn)
							break;

						newX = match.Index;
					}
				}

				// :: new position is not found in the line
				if (newX < 0)
				{
					if (alt || line != null)
						return;

					if (right)
					{
						if (++iLine >= editor.Count)
							return;

						iColumn = -1;
					}
					else
					{
						if (--iLine < 0)
							return;

						iColumn = int.MaxValue;
					}

					continue;
				}

				if (operation == Operation.Step)
				{
					// :: step
					if (line == null)
					{
						editor.GoTo(newX, iLine);
						editor.UnselectText();
						editor.Redraw();
					}
					else
					{
						//_100819_142053 Mantis#1464 Here was a kludge

						line.UnselectText();
						line.Caret = newX;
					}
				}
				else if (operation == Operation.Select)
				{
					// :: select
					if (alt)
						SelectColumn(editor, right, iLine, caret.X, newX);
					else
						SelectStream(editor, line, right, caret, new Point(newX, iLine));
				}
				else
				{
					// :: delete
					if (line == null)
					{
						if (!right && newX == 0 && caret.X > 0 && currentLine.Length == 0)
						{
							// "Cursor beyond end of line" and the line is empty
							editor.UnselectText();
							editor.GoToColumn(0);
						}
						else
						{
							// select the step text and delete it
							if (!editor.SelectionExists)
							{
								if (right)
									editor.SelectText(caret.X, caret.Y, newX - 1, iLine);
								else
									editor.SelectText(newX, iLine, caret.X - 1, caret.Y);
							}
							editor.DeleteText();
						}
						editor.Redraw();
					}
					else
					{
						if (line.SelectionSpan.Length <= 0)
						{
							if (right)
								line.SelectText(caret.X, newX);
							else
								line.SelectText(newX, caret.X);
						}
						newX = line.SelectionSpan.Start;
						line.SelectedText = string.Empty;
						line.Caret = newX;
					}
				}

				return;
			}
		}
		static void SelectStream(IEditor editor, ILine line, bool right, Point caretOld, Point caretNew)
		{
			Point first, last;
			if (editor != null && editor.SelectionExists || line != null && line.SelectionSpan.Length > 0)
			{
				Place place;
				if (line == null)
				{
					place = editor.SelectionPlace;
				}
				else
				{
					var span = line.SelectionSpan;
					place = new Place(span.Start, 0, span.Start + span.Length - 1, 0);
				}

				if (right)
				{
					if (place.Last == new Point(caretNew.X - 1, caretNew.Y))
					{
						// vanish selection
						first = last = new Point(-1, -1);
					}
					else if (caretOld != place.First)
					{
						// expand selection
						first = place.First;
						last = new Point(caretNew.X - 1, caretNew.Y);
					}
					else if (caretNew.Y > place.Last.Y || caretNew.Y == place.Last.Y && caretNew.X > place.Last.X)
					{
						// invert selection
						first = new Point(place.Last.X + 1, place.Last.Y);
						last = new Point(caretNew.X - 1, caretNew.Y);
					}
					else
					{
						// reduce selection
						first = caretNew;
						last = place.Last;
					}
				}
				else
				{
					if (place.First == caretNew)
					{
						// vanish selection
						first = last = new Point(-1, -1);
					}
					else if (place.Last != new Point(caretOld.X - 1, caretOld.Y))
					{
						// expand selection
						first = caretNew;
						last = place.Last;
					}
					else if (caretNew.Y < place.First.Y || caretNew.Y == place.First.Y && caretNew.X < place.First.X)
					{
						// invert selection
						first = caretNew;
						last = new Point(place.First.X - 1, place.First.Y);
					}
					else
					{
						// reduce selection
						first = place.First;
						last = new Point(caretNew.X - 1, caretNew.Y);
					}
				}
			}
			else
			{
				// start selection
				if (right)
				{
					first = caretOld;
					last = new Point(caretNew.X - 1, caretNew.Y);
				}
				else
				{
					first = caretNew;
					last = new Point(caretOld.X - 1, caretOld.Y);
				}
			}

			// set/drop selection and set the caret
			if (line == null)
			{
				editor.GoTo(caretNew.X, caretNew.Y);
				if (first.Y >= 0)
					editor.SelectText(first.X, first.Y, last.X, last.Y);
				else
					editor.UnselectText();
				editor.Redraw();
			}
			else
			{
				line.Caret = caretNew.X;
				if (first.Y >= 0)
					line.SelectText(first.X, last.X + 1);
				else
					line.UnselectText();
			}
		}
		static void SelectColumn(IEditor editor, bool right, int line, int caretOld, int caretNew)
		{
			int x1, y1, x2, y2;
			if (editor.SelectionExists)
			{
				if (editor.SelectionKind != PlaceKind.Column)
					return;

				// editor selection
				var place = editor.SelectionPlace;
				y1 = place.First.Y;
				y2 = place.Last.Y;

				// screen selection and carets
				int select1 = editor.ConvertColumnEditorToScreen(y1, place.First.X);
				int select2 = editor.ConvertColumnEditorToScreen(y2, place.Last.X);
				int caret1 = editor.ConvertColumnEditorToScreen(line, caretOld);
				int caret2 = editor.ConvertColumnEditorToScreen(line, caretNew);

				if (right)
				{
					if (caret1 < select2 && caret2 > select2)
					{
						// invert selection
						x1 = select2 + 1;
						x2 = caret2 - 1;
					}
					else if (caret1 != select1)
					{
						// expand selection
						x1 = select1;
						x2 = caret2 - 1;
					}
					else
					{
						// reduce selection
						x1 = caret2;
						x2 = select2;
					}
				}
				else
				{
					if (caret2 >= select1)
					{
						// reduce selection
						x1 = select1;
						x2 = caret2 - 1;
					}
					else if (caret1 > select1)
					{
						// invert selection
						x1 = caret2;
						x2 = select1 - 1;
					}
					else
					{
						// expand selection
						x1 = caret2;
						x2 = select2;
					}
				}
			}
			else
			{
				// start selection
				y1 = y2 = line;
				if (right)
				{
					x1 = editor.ConvertColumnEditorToScreen(y1, caretOld);
					x2 = editor.ConvertColumnEditorToScreen(y1, caretNew - 1);
				}
				else
				{
					x1 = editor.ConvertColumnEditorToScreen(y1, caretNew);
					x2 = editor.ConvertColumnEditorToScreen(y1, caretOld - 1);
				}
			}

			// set/drop selection and set the caret
			editor.GoTo(caretNew, line);
			editor.SelectText(x1, y1, x2, y2, PlaceKind.Column);
			editor.Redraw();
		}
		// This should be removed when Mantis 1465 is resolved.
		void SelectWorkaround(ILine line, bool right)
		{
			int oldX = line.Caret;
			int newX;
			if (right)
			{
				newX = oldX + 1;
				if (newX > line.Length)
					return;
			}
			else
			{
				newX = oldX - 1;
				if (newX < 0)
					return;
			}

			SelectStream(null, line, right, new Point(oldX, 0), new Point(newX, 0));
		}
		/// <summary>
		/// Moves the caret or selects the text to the smart line home.
		/// </summary>
		/// <param name="editor">The current editor or null.</param>
		/// <param name="line">The active line or null.</param>
		/// <param name="select">True for selection, false for move.</param>
		/// <remarks>
		/// Smart line home is the first not white space line position
		/// if the caret is not there or the standard line home otherwise.
		/// </remarks>
		static void Home(IEditor editor, ILine line, bool select)
		{
			if (editor != null)
				line = editor[-1];

			int home = 0;
			int caret = line.Caret;
			string text = line.Text;
			Match match = Regex.Match(text, @"^(\s+)");
			if (match.Success)
				home = match.Groups[1].Length;

			if (select)
			{
				var span = line.SelectionSpan;
				if (span.Start < 0)
				{
					if (caret < home)
					{
						line.SelectText(caret, home);
						line.Caret = home;
					}
					else if (caret > home)
					{
						line.SelectText(home, caret);
						line.Caret = home;
					}
					else
					{
						line.SelectText(0, home);
						line.Caret = 0;
					}
				}
				else if (span.Start == 0 && span.End == home)
				{
					line.UnselectText();
					if (caret == 0)
						line.Caret = home;
					else
						line.Caret = 0;
				}
				else if (span.Start > 0 && span.End == home)
				{
					line.SelectText(0, span.Start);
					line.Caret = 0;
				}
				else if (span.Start == 0 && span.End < home)
				{
					line.SelectText(span.End, home);
					line.Caret = home;
				}
				else if (span.Start > 0 && span.End < home)
				{
					if (caret == span.Start)
						line.SelectText(span.End, home);
					else
						line.SelectText(span.Start, home);
					line.Caret = home;
				}
				else
				{
					if (home == caret)
						home = 0;

					line.SelectText(home, span.End);
					line.Caret = home;
				}
			}
			else
			{
				// go to smart home
				line.UnselectText();
				line.Caret = caret == home ? 0 : home;
			}

			if (editor != null)
				editor.Redraw();
		}
	}
}
