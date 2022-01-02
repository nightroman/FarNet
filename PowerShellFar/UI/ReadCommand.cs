
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Management.Automation;
using System.Threading.Tasks;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class ReadCommand
	{
		const string DefaultPrompt = "PS> ";

		internal static bool Exit { get; set; }
		static bool _visiblePanel1;
		static bool _visiblePanel2;
		static bool _visibleKeyBar;
		static Func<bool> _show;

		IDialog _Dialog { get; set; }
		IEdit _Edit { get; set; }
		string _TextFromEditor;

		ReadCommand(string prompt, string history)
		{
			Far.Api.UI.WindowTitle = prompt;
			var size = Far.Api.UI.WindowSize;

			int maxPromptLength = size.X / 3;
			bool tooLong = prompt.Length > maxPromptLength;
			var pos = tooLong ? maxPromptLength : prompt.Length;

			_Dialog = Far.Api.CreateDialog(0, size.Y - 1, size.X - 1, size.Y - 1);
			_Dialog.TypeId = new(Guids.ReadCommandDialog);
			_Dialog.NoShadow = true;
			_Dialog.KeepWindowTitle = true;

			//! make 1 wider, to hide the arrow
			_Edit = _Dialog.AddEdit(pos, 0, size.X - 1, string.Empty);
			_Edit.IsPath = true;
			_Edit.History = history;
			_Edit.Coloring += Coloring.ColorEditAsConsole;

			if (tooLong)
			{
				var uiText = _Dialog.AddEdit(0, 0, pos - 1, prompt.TrimEnd());
				uiText.ReadOnly = true;
				uiText.Coloring += Coloring.ColorEditAsConsole;
				uiText.LosingFocus += (sender, e) =>
				{
					uiText.Line.Caret = -1;
				};
			}
			else
			{
				var uiText = _Dialog.AddText(0, 0, pos - 1, prompt);
				uiText.Coloring += Coloring.ColorTextAsConsole;
			}

			// hotkeys
			_Edit.KeyPressed += OnKey;

			// ignore clicks outside
			_Dialog.MouseClicked += (sender, e) =>
			{
				if (e.Control == null)
					e.Ignore = true;
			};
		}

		void OnKey(object sender, KeyPressedEventArgs e)
		{
			switch (e.Key.VirtualKeyCode)
			{
				case KeyCode.Escape:
					if (_Edit.Line.Length > 0)
					{
						e.Ignore = true;
						_Edit.Text = string.Empty;
					}
					break;
				case KeyCode.Tab:
					if (e.Key.Is())
					{
						e.Ignore = true;
						EditorKit.ExpandCode(_Edit.Line, null);
					}
					break;
				case KeyCode.UpArrow:
				case KeyCode.DownArrow:
					if (e.Key.Is())
					{
						e.Ignore = true;
						_Edit.Text = History.GetNextCommand(e.Key.VirtualKeyCode == KeyCode.UpArrow, _Edit.Text);
						_Edit.Line.Caret = -1;
					}
					break;
				case KeyCode.F1:
					var text = _Edit.Line.Text;
					var caret = _Edit.Line.Caret;
					_show = () =>
					{
						//! do modal or Far crashes with editor or viewer and user screen ops
						Help.ShowHelpForText(text, caret, HelpTopic.CommandConsole, OpenMode.Modal);
						return false;
					};
					e.Ignore = true;
					_Dialog.Close();
					break;
				case KeyCode.F2:
					e.Ignore = true;
					Far.Api.PostMacro("mf.usermenu(0, '')");
					break;
				case KeyCode.F4:
					_show = () =>
					{
						var args = new EditTextArgs() { Text = _Edit.Text, Title = "Input code", Extension = "psm1" };
						var text = Far.Api.AnyEditor.EditText(args);
						if (text == args.Text)
							return false;

						_TextFromEditor = text;
						return true;
					};
					e.Ignore = true;
					_Dialog.Close();
					break;
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

		/// <summary>
		/// (!) Run it from the main.
		/// </summary>
		public static async Task RunAsync()
		{
			// must be panels
			if (Far.Api.Window.Kind != WindowKind.Panels)
				throw new InvalidOperationException("Command console should start from panels.");

			// already started
			if (A.IsCommandMode)
				return;

			// save visibility of panels and key bar
			_visiblePanel1 = Far.Api.Panel.IsVisible;
			_visiblePanel2 = Far.Api.Panel2.IsVisible;
			_visibleKeyBar = Console.CursorTop - Console.WindowTop == Console.WindowHeight - 2;

			// hide panels and key bar
			if (_visiblePanel1 || _visiblePanel2)
				await Tasks.Keys("CtrlO");
			if (_visibleKeyBar)
				await Tasks.Keys("CtrlB");

			// post command loop
			await Tasks.Job(StartLoop);
		}

		static void StartLoop()
		{
			//! hide menu bar
			Far.Api.UI.ShowUserScreen();

			A.IsCommandMode = true;
			try
			{
				Loop(false);
			}
			finally
			{
				A.IsCommandMode = false;

				// scroll one line
				if (_visibleKeyBar)
				{
					var textUnderPrompt = Far.Api.UI.GetBufferLineText(-2);
					if (textUnderPrompt.Length > 0)
						Far.Api.UI.WriteLine();
				}

				//! [CtrlO] works but there are issues on testing
				Far.Api.UI.SetUserScreen(0);
				Far.Api.Panel.IsVisible = _visiblePanel1; //! 1st
				Far.Api.Panel2.IsVisible = _visiblePanel2; //! 2nd
				if (_visibleKeyBar)
					Far.Api.PostMacro("Keys'CtrlB'");
			}
		}

		internal static void Loop(bool nested)
		{
			// reset the flag for this loop
			Exit = false;

			// REPL
			for (; ; )
			{
				// get prompt
				var prompt = GetPrompt();

				// show prompt
				var ui = new ReadCommand(prompt, Res.History);
				Far.Api.UI.SetUserScreen(Far.Api.UI.SetUserScreen(0));
				bool yes = ui._Dialog.Show();
				if (!yes)
				{
					if (nested)
					{
						// user exists UI, not by "exit", so do "exit"
						using var ps = A.Psf.NewPowerShell();
						ps.AddScript("exit").Invoke();
					}
					break;
				}

				// editor
				if (_show != null)
				{
					//! or editor makes noise, e.g. Colorer "Reloading..." message, [1]
					int screen = Far.Api.UI.SetUserScreen(0);

					yes = _show();

					//! do after [1] or it shows panels
					Far.Api.UI.SetUserScreen(screen);

					_show = null;
					if (!yes)
						continue;
				}

				// get code, skip empty
				bool fromEditor = ui._TextFromEditor != null;
				var code = (fromEditor ? ui._TextFromEditor : ui._Edit.Text).TrimEnd();
				if (code.Length == 0)
					continue;

				// editor - do not add 2+ lines to history
				// prompt - add to history
				var args = new RunArgs(code);
				if (fromEditor)
				{
					args.Writer = new ConsoleOutputWriter($"{prompt}\n{code}", true);
					args.AddHistory = !code.Contains("\n");
				}
				else
				{
					args.Writer = new ConsoleOutputWriter(prompt + code, true);
					args.AddHistory = true;
				}
				A.Psf.Run(args);

				// "exit" is called and set by ExitNestedPrompt()
				if (Exit)
					break;
			}

			// reset the flag for other loops
			Exit = false;
		}
	}
}
