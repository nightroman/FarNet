
/*
FarNet module RightControl
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Text.RegularExpressions;
using FarNet.Forms;

namespace FarNet.RightControl
{
	[System.Runtime.InteropServices.Guid("cb0fd385-474d-41de-ad42-c6d4d0d65b3d")]
	[ModuleTool(Name = "RightControl", Options = ModuleToolOptions.Editor | ModuleToolOptions.Dialog | ModuleToolOptions.Panels)]
	public class RightControlTool : ModuleTool
	{
		const string DefaultPattern = @"^ | $ | (?<=\b|\s)\S";
		static Regex _regex_;
		static IMenu _menu;

		static Regex Regex
		{
			get
			{
				if (_regex_ == null)
					InitRegex(null);
				return _regex_;
			}
		}

		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			InitRegex(Manager);

			ILine line = null;
			IEditor editor = null;
			if (e.From == ModuleToolOptions.Editor)
			{
				editor = Far.Net.Editor;
			}
			else
			{
				line = Far.Net.Line;
				if (line == null)
					return;
			}

			if (_menu == null)
			{
				_menu = Far.Net.CreateMenu();
				_menu.Title = "RightControl";
				_menu.Add("&1. step left");
				_menu.Add("&2. step right");
				_menu.Add("&3. select left");
				_menu.Add("&4. select right");
				_menu.Add("&5. delete left");
				_menu.Add("&6. delete right");
				_menu.Add("&7. vertical left");
				_menu.Add("&8. vertical right");
				_menu.Add("&h. go to smart home");
				_menu.Add("&s. select to smart home");
				_menu.Lock();
			}

			_menu.Show();
			switch (_menu.Selected)
			{
				case 0: Run(editor, line, Operation.Step, false, false); break;
				case 1: Run(editor, line, Operation.Step, true, false); break;
				case 2: Run(editor, line, Operation.Select, false, false); break;
				case 3: Run(editor, line, Operation.Select, true, false); break;
				case 4: Run(editor, line, Operation.Delete, false, false); break;
				case 5: Run(editor, line, Operation.Delete, true, false); break;
				case 6:
					if (editor == null)
						SelectWorkaround(line, false);
					else
						Run(editor, line, Operation.Select, false, true);
					break;
				case 7:
					if (editor == null)
						SelectWorkaround(line, true);
					else
						Run(editor, line, Operation.Select, true, true);
					break;
				case 8: Home(editor, line, false); break;
				case 9: Home(editor, line, true); break;
			}
		}

		static void InitRegex(IModuleManager manager)
		{
			if (_regex_ != null)
				return;

			string pattern = DefaultPattern;
			if (manager != null)
			{
				using (var key = manager.OpenRegistryKey(null, false))
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
			}

			try
			{
				_regex_ = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
			}
			catch (Exception e)
			{
				Far.Net.Message("Error on parsing the regular expression:\r" + e.Message, "RightControl", MsgOptions.LeftAligned | MsgOptions.Warning);
				_regex_ = new Regex(DefaultPattern, RegexOptions.IgnorePatternWhitespace);
			}
		}

		/// <summary>
		/// Operation kind.
		/// </summary>
		public enum Operation
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
		public static void Run(IEditor editor, ILine line, Operation operation, bool right, bool alt)
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
						//_100819_142053 Workaround
						if (line.WindowKind == WindowKind.Dialog)
						{
							IDialog dialog = Far.Net.Dialog;
							if (dialog != null)
							{
								var control = dialog.Focused as IEditable;
								if (control != null)
									control.IsTouched = true;
							}
						}

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
				if (first.Y >= 0)
					editor.SelectText(first.X, first.Y, last.X, last.Y);
				else
					editor.UnselectText();
				editor.GoTo(caretNew.X, caretNew.Y);
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
			editor.SelectText(x1, y1, x2, y2, PlaceKind.Column);
			editor.GoTo(caretNew, line);
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
		public static void Home(IEditor editor, ILine line, bool select)
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
