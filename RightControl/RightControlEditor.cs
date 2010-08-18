
/*
FarNet module RightControl
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Text.RegularExpressions;

namespace FarNet.RightControl
{
	[System.Runtime.InteropServices.Guid("65c2c1ec-cb83-446b-bddc-1e6ac8c2436b")]
	[ModuleEditor(Name = "RightControl")]
	public class RightControlEditor : ModuleEditor
	{
		const string DefaultPattern = @"^ | $ | (?<=\b|\s)\S";
		static Regex _regex;

		public override void Invoke(object sender, ModuleEditorEventArgs e)
		{
			InitRegex();
			((IEditor)sender).KeyDown += OnKeyDown;
		}

		void InitRegex()
		{
			if (_regex != null)
				return;

			string pattern = DefaultPattern;
			using (var key = Manager.OpenRegistryKey(null, false))
			{
				if (key != null)
				{
					object value = key.GetValue("Regex", DefaultPattern);
					if (value is string[])
						pattern = string.Join(Environment.NewLine, (string[])value);
					else
						pattern = value.ToString();
				}
			}

			try
			{
				_regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
			}
			catch (Exception e)
			{
				Far.Net.Message("Error on parsing the regular expression:\r" + e.Message, "RightControl", MsgOptions.LeftAligned | MsgOptions.Warning);
				_regex = new Regex(DefaultPattern, RegexOptions.IgnorePatternWhitespace);
			}
		}

		static void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key.VirtualKeyCode)
			{
				case VKeyCode.LeftArrow:
					if (e.Key.CtrlAltShift == ControlKeyStates.LeftCtrlPressed)
						Run((IEditor)sender, Operation.Step, false, false);
					else if (e.Key.CtrlAltShift == (ControlKeyStates.LeftCtrlPressed | ControlKeyStates.ShiftPressed))
						Run((IEditor)sender, Operation.Select, false, false);
					else if (e.Key.CtrlAltShift == (ControlKeyStates.LeftCtrlPressed | ControlKeyStates.LeftAltPressed))
						Run((IEditor)sender, Operation.Select, false, true);
					else
						return;
					e.Ignore = true;
					break;
				case VKeyCode.RightArrow:
					if (e.Key.CtrlAltShift == ControlKeyStates.LeftCtrlPressed)
						Run((IEditor)sender, Operation.Step, true, false);
					else if (e.Key.CtrlAltShift == (ControlKeyStates.LeftCtrlPressed | ControlKeyStates.ShiftPressed))
						Run((IEditor)sender, Operation.Select, true, false);
					else if (e.Key.CtrlAltShift == (ControlKeyStates.LeftCtrlPressed | ControlKeyStates.LeftAltPressed))
						Run((IEditor)sender, Operation.Select, true, true);
					else
						return;
					e.Ignore = true;
					break;
				case VKeyCode.Backspace:
					if (e.Key.CtrlAltShift == ControlKeyStates.LeftCtrlPressed)
					{
						Run((IEditor)sender, Operation.Delete, false, false);
						e.Ignore = true;
					}
					break;
				case VKeyCode.Delete:
					if (e.Key.CtrlAltShift == ControlKeyStates.LeftCtrlPressed)
					{
						Run((IEditor)sender, Operation.Delete, true, false);
						e.Ignore = true;
					}
					break;
			}
		}

		enum Operation
		{
			Step,
			Select,
			Delete
		}

		static void Run(IEditor editor, Operation operation, bool right, bool alt)
		{
			Point caret = editor.Caret;
			int iColumn = caret.X;
			int iLine = caret.Y;
			for (; ; )
			{
				ILine line = editor[iLine];
				var text = line.Text;

				int newX = -1;
				foreach (Match match in _regex.Matches(text))
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

				if (newX >= 0)
				{
					if (operation == Operation.Select)
					{
						// select
						if (alt)
							SelectColumn(editor, right, iLine, caret.X, newX);
						else
							SelectStream(editor, right, caret, new Point(newX, iLine));
					}
					else if (operation == Operation.Delete)
					{
						// delete
						if (!editor.SelectionExists)
						{
							if (right)
								editor.SelectText(caret.X, caret.Y, newX - 1, iLine);
							else
								editor.SelectText(newX, iLine, caret.X - 1, caret.Y);
						}
						editor.DeleteText();
						editor.Redraw();
					}
					else
					{
						// set the caret and drop selection
						editor.GoTo(newX, iLine);
						editor.UnselectText();
						editor.Redraw();
					}
					return;
				}

				if (alt)
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
			}
		}

		static void SelectStream(IEditor editor, bool right, Point caretOld, Point caretNew)
		{
			Point first, last;
			if (editor.SelectionExists)
			{
				var place = editor.SelectionPlace;
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
			if (first.Y >= 0)
				editor.SelectText(first.X, first.Y, last.X, last.Y);
			else
				editor.UnselectText();
			editor.GoTo(caretNew.X, caretNew.Y);
			editor.Redraw();
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
			editor.SelectText(x1, y1, x2, y2, PlaceKind.Column);
			editor.GoTo(caretNew, line);
			editor.Redraw();
		}
	
	}
}
