
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

		IDialog UIDialog { get; set; }
		IEdit UIEdit { get; set; }
		int _Caret = -1;

		InputConsole(string prompt, string history)
		{
			var size = Far.Api.UI.WindowSize;

			// shorten prompt
			if (prompt.Length > size.X / 2)
			{
				int n = size.X / 4 - 2;
				prompt = prompt.Substring(0, n) + " .. " + prompt.Substring(prompt.Length - n, n);
			}

			UIDialog = Far.Api.CreateDialog(0, size.Y - 2, size.X - 1, size.Y - 1);
			UIDialog.NoShadow = true;

			var UIText = UIDialog.AddText(0, 0, prompt.Length - 1, prompt);

			UIEdit = UIDialog.AddEdit(prompt.Length, 0, size.X - 2, string.Empty);
			UIEdit.History = history;

			var UIArea = UIDialog.AddText(0, 1, size.X - 1, string.Empty);

			// color text
			EventHandler<ColoringEventArgs> colorText = (sender, e) =>
			{
				// normal text
				e.Background1 = ConsoleColor.Black;
				e.Foreground1 = ConsoleColor.Gray;
			};
			UIText.Coloring += colorText;
			UIArea.Coloring += colorText;

			// color edit
			UIEdit.Coloring += (sender, e) =>
			{
				// normal text
				e.Background1 = ConsoleColor.Black;
				e.Foreground1 = ConsoleColor.Gray;
				// selected text
				e.Background2 = ConsoleColor.White;
				e.Foreground2 = ConsoleColor.DarkGray;
				// unchanged text
				e.Background3 = ConsoleColor.Black;
				e.Foreground3 = ConsoleColor.Red;
				// combo
				e.Background4 = ConsoleColor.Black;
				e.Foreground4 = ConsoleColor.Gray;
			};

			// set caret
			UIEdit.GotFocus += (sender, e) =>
			{
				if (_Caret >= 0)
				{
					UIEdit.IsTouched = true;
					UIEdit.Line.Caret = _Caret;
					_Caret = -1;
				}
			};

			// hotkeys
			UIEdit.KeyPressed += OnKey;
		}
		void OnKey(object sender, KeyPressedEventArgs e)
		{
			switch (e.Key.VirtualKeyCode)
			{
				case KeyCode.Enter:
					// [Enter]
					_Caret = UIEdit.Line.Caret;
					break;
				case KeyCode.Escape:
					// [Escape]
					if (UIEdit.Line.Length > 0)
					{
						e.Ignore = true;
						UIEdit.Text = "";
					}
					break;
				case KeyCode.Tab:
					// [Tab]
					e.Ignore = true;
					EditorKit.ExpandCode(UIEdit.Line, null);
					break;
				case KeyCode.F1:
					// [F1]
					e.Ignore = true;
					Help.ShowHelpForContext("CommandInputLine");
					break;
				case KeyCode.UpArrow:
					// [UpArrow]
					if (e.Key.Is())
					{
						e.Ignore = true;
						OnHistory(-1);
					}
					break;
				case KeyCode.DownArrow:
					// [UpArrow]
					if (e.Key.Is())
					{
						e.Ignore = true;
						OnHistory(1);
					}
					break;
			}
		}
		void OnHistory(int direction)
		{
			string lastUsedCmd = null;
			if (History.Cache == null) //TODO duplicated code
			{
				lastUsedCmd = UIEdit.Text;
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
				UIEdit.Text = code;
				UIEdit.Line.Caret = -1;
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
				string code = "";
				int caret = -1;
				for (; ; )
				{
					var prompt = GetPrompt();

					var ui = new InputConsole(prompt, Res.History);
					ui.UIEdit.Text = code;
					ui._Caret = caret;

					if (!ui.UIDialog.Show())
						return;

					code = ui.UIEdit.Text.TrimEnd();
					caret = ui._Caret;
					if (code.Length == 0)
						continue;

					// invoke, add to history
					if (A.Psf.Act(code, new ConsoleOutputWriter(prompt + code), true))
					{
						code = "";
						caret = -1;
					}
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
