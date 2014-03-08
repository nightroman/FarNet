
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Management.Automation;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class InputConsole
	{
		const string DefaultPrompt = "PS> ";

		static bool _visiblePanel1;
		static bool _visiblePanel2;
		static bool _visibleKeyBar;

		IDialog _Dialog { get; set; }
		IEdit _Edit { get; set; }
		string _TextFromEditor;

		InputConsole(string prompt, string history)
		{
			Far.Api.UI.WindowTitle = prompt;
			var size = Far.Api.UI.WindowSize;

			bool tooLong = prompt.Length > size.X / 2;
			var pos = tooLong ? size.X / 2 : prompt.Length;

			_Dialog = Far.Api.CreateDialog(0, size.Y - 2, size.X - 1, size.Y - 1);
			_Dialog.NoShadow = true;
			_Dialog.KeepWindowTitle = true;

			_Edit = _Dialog.AddEdit(pos, 0, size.X - 2, string.Empty);
			_Edit.History = history;
			_Edit.Coloring += ColorEdit;

			if (tooLong)
			{
				var UIText = _Dialog.AddEdit(0, 0, pos - 1, prompt.TrimEnd());
				UIText.ReadOnly = true;
				UIText.Coloring += ColorEdit;
				UIText.LosingFocus += (sender, e) =>
					{
						UIText.Line.Caret = -1;
					};
			}
			else
			{
				var UIText = _Dialog.AddText(0, 0, pos - 1, prompt);
				UIText.Coloring += ColorText;
			}

			var UIArea = _Dialog.AddText(0, 1, size.X - 1, string.Empty);
			UIArea.Coloring += ColorText;

			// hotkeys
			_Edit.KeyPressed += OnKey;

			// ignore clicks outside
			_Dialog.MouseClicked += (sender, e) =>
				{
					if (e.Control == null)
						e.Ignore = true;
				};
		}
		void ColorEdit(object sender, ColoringEventArgs e)
		{
			// normal text
			e.Background1 = ConsoleColor.Black;
			e.Foreground1 = ConsoleColor.Gray;
			// selected text
			e.Background2 = ConsoleColor.White;
			e.Foreground2 = ConsoleColor.DarkGray;
			// unchanged text
			e.Background3 = ConsoleColor.Black;
			e.Foreground3 = ConsoleColor.Gray;
			// combo
			e.Background4 = ConsoleColor.Black;
			e.Foreground4 = ConsoleColor.Gray;
		}
		void ColorText(object sender, ColoringEventArgs e)
		{
			// normal text
			e.Background1 = ConsoleColor.Black;
			e.Foreground1 = ConsoleColor.Gray;
		}
		void OnKey(object sender, KeyPressedEventArgs e)
		{
			switch (e.Key.VirtualKeyCode)
			{
				case KeyCode.Escape:
					if (_Edit.Line.Length > 0)
					{
						e.Ignore = true;
						_Edit.Text = "";
					}
					break;
				case KeyCode.Tab:
					if (e.Key.Is())
					{
						e.Ignore = true;
						EditorKit.ExpandCode(_Edit.Line, null);
					}
					break;
				case KeyCode.F1:
					e.Ignore = true;
					Help.ShowHelpForContext("CommandConsoleDialog");
					break;
				case KeyCode.UpArrow:
					if (e.Key.Is())
					{
						e.Ignore = true;
						OnHistory(-1);
					}
					break;
				case KeyCode.DownArrow:
					if (e.Key.Is())
					{
						e.Ignore = true;
						OnHistory(1);
					}
					break;
				case KeyCode.F4:
					e.Ignore = true;
					var args = new EditTextArgs() { Text = _Edit.Text, Title = "Input code", Extension = "psm1" };
					var text = Far.Api.AnyEditor.EditText(args);
					if (text != args.Text)
					{
						_TextFromEditor = text;
						_Dialog.Close();
					}
					break;
			}
		}
		void OnHistory(int direction)
		{
			string lastUsedCmd = null;
			if (History.Cache == null) //TODO duplicated code
			{
				lastUsedCmd = _Edit.Text;
				History.Cache = History.ReadLines();
				History.CacheIndex = History.Cache.Length;
			}
			else if (History.CacheIndex >= 0 && History.CacheIndex < History.Cache.Length)
			{
				lastUsedCmd = History.Cache[History.CacheIndex];
			}
			string code;
			if (direction < 0)
			{
				for (; ; )
				{
					if (--History.CacheIndex < 0)
					{
						code = string.Empty;
						History.CacheIndex = -1;
					}
					else
					{
						code = History.Cache[History.CacheIndex];
						if (code == lastUsedCmd)
							continue;
					}
					break;
				}
			}
			else
			{
				for (; ; )
				{
					if (++History.CacheIndex >= History.Cache.Length)
					{
						code = string.Empty;
						History.CacheIndex = History.Cache.Length;
					}
					else
					{
						code = History.Cache[History.CacheIndex];
						if (code == lastUsedCmd)
							continue;
					}
					break;
				}
			}

			if (code != null)
			{
				_Edit.Text = code;
				_Edit.Line.Caret = -1;
			}
		}
		//! like PS, use just result[0] if it is not empty
		static string GetPrompt()
		{
			try
			{
				using (var ps = A.Psf.NewPowerShell())
				{
					var r = ps.AddCommand("prompt").Invoke();

					string prompt;
					if (r.Count > 0 && r[0] != null && (prompt = r[0].ToString()).Length > 0)
						return prompt;
				}
			}
			catch (RuntimeException) { }

			return DefaultPrompt;
		}
		public static void Start()
		{
			// fail: must be panels
			if (Far.Api.Window.Kind != WindowKind.Panels)
				throw new InvalidOperationException("Command console must be started from panels.");

			// exit: already started
			if (Far.Api.UI.IsCommandMode)
				return;

			// save visibility of panels and key bar
			_visiblePanel1 = Far.Api.Panel.IsVisible;
			_visiblePanel2 = Far.Api.Panel2.IsVisible;
			_visibleKeyBar = Console.CursorTop - Console.WindowTop == Console.WindowHeight - 2;

			// post hide panels and key bar
			if (_visiblePanel1 || _visiblePanel2)
				Far.Api.PostMacro("Keys'CtrlO'");
			if (_visibleKeyBar)
				Far.Api.PostMacro("Keys'CtrlB'");

			// post command loop
			Far.Api.PostStep(Loop);
		}
		static void Loop()
		{
			Far.Api.UI.ShowUserScreen(); //! to hide menu bar
			Far.Api.UI.IsCommandMode = true;
			try
			{
				for (; ; )
				{
					var prompt = GetPrompt();

					var ui = new InputConsole(prompt, Res.History);
					if (!ui._Dialog.Show())
						return;

					bool fromEditor = ui._TextFromEditor != null;
					var code = (fromEditor ? ui._TextFromEditor : ui._Edit.Text).TrimEnd();
					if (code.Length == 0)
						continue;

					// code from editor - invoke, do not add to history
					// code from line - invoke, add to history
					if (fromEditor)
						A.Psf.Act(code, new ConsoleOutputWriter(prompt + Environment.NewLine + code, true), false);
					else
						A.Psf.Act(code, new ConsoleOutputWriter(prompt + code, true), true);
				}
			}
			finally
			{
				Far.Api.UI.IsCommandMode = false;

				//! "Keys'CtrlO'" works but there are issues on testing
				Far.Api.Panel.IsVisible = _visiblePanel1; //! 1st
				Far.Api.Panel2.IsVisible = _visiblePanel2; //! 2nd
				if (_visibleKeyBar)
					Far.Api.PostMacro("Keys'CtrlB'");
			}
		}
	}
}