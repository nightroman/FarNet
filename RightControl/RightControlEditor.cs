
/*
FarNet module RightControl
Copyright (c) 2010 Roman Kuzmin
*/

using System.Text.RegularExpressions;
using FarNet;

namespace RightControl
{
	[System.Runtime.InteropServices.Guid("65c2c1ec-cb83-446b-bddc-1e6ac8c2436b")]
	[ModuleEditor(Name = "RightControl")]
	public class RightControlEditor : ModuleEditor
	{
		public override void Invoke(object sender, ModuleEditorEventArgs e)
		{
			((IEditor)sender).KeyDown += OnKeyDown;
		}

		void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key.VirtualKeyCode)
			{
				case VKeyCode.LeftArrow:
					if (e.Key.CtrlAltShift == ControlKeyStates.LeftCtrlPressed)
						Run((IEditor)sender, false, false);
					else if (e.Key.CtrlAltShift == (ControlKeyStates.LeftCtrlPressed | ControlKeyStates.ShiftPressed))
						Run((IEditor)sender, false, true);
					else
						return;
					e.Ignore = true;
					break;
				case VKeyCode.RightArrow:
					if (e.Key.CtrlAltShift == ControlKeyStates.LeftCtrlPressed)
						Run((IEditor)sender, true, false);
					else if (e.Key.CtrlAltShift == (ControlKeyStates.LeftCtrlPressed | ControlKeyStates.ShiftPressed))
						Run((IEditor)sender, true, true);
					else
						return;
					e.Ignore = true;
					break;
			}
		}

		static void Run(IEditor editor, bool right, bool select)
		{
			Regex re = new Regex(@"^|$|\b[^\s]|(?<=\s)\S");
			Point caret = editor.Caret;
			int iColumn = caret.X;
			int iLine = caret.Y;
			for (; ; )
			{
				ILine line = editor[iLine];
				var text = line.Text;

				int newX = -1;
				foreach (Match match in re.Matches(text))
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
					if (select)
					{
						Select(editor, right, caret, new Point(newX, iLine));
					}
					else
					{
						editor.GoTo(newX, iLine);
						editor.Redraw();
					}
					return;
				}

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

		static void Select(IEditor editor, bool right, Point caretOld, Point caretNew)
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
	}
}
