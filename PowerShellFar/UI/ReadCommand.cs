using FarNet;
using FarNet.Forms;
using System.Diagnostics;

namespace PowerShellFar.UI;

internal sealed class ReadCommand
{
	private const string ErrorCannotSetPanels = "Cannot set area 'Panels'.";

	private static ReadCommand? Instance { get; set; }
	private readonly ReadCommandForm _form;

	private static readonly bool __isConsoleMode = A.FAR_PWSF_MODE && !A.FAR_PWSF_PANELS;

	public ReadCommand()
	{
		_form = new ReadCommandForm();
		_form.Dialog.KeyPressed += OnKeyPressed;

		// with no panels, set area Desktop to hide editor / viewer
		if (!Far.Api.HasPanels && Far.Api.Window.Kind != WindowKind.Desktop)
			Far.Api.Window.SetCurrentAt(0);
	}

	static ReadCommand()
	{
		if (__isConsoleMode)
		{
			Far.Api.AnyEditor.Closed += OnWindow;
			Far.Api.AnyViewer.Closed += OnWindow;
		}
	}

	public static bool IsActive()
	{
		if (Instance is null)
			return false;

		var from = Far.Api.Window.Kind;
		if (from == WindowKind.Desktop)
			return true;

		return from == WindowKind.Dialog && Far.Api.Dialog!.TypeId == ReadCommandForm.MyTypeId;
	}

	public static void Stop()
	{
		Instance?._form.Close();
	}

	private static void OnWindow(object? sender, EventArgs e)
	{
		Far.Api.PostJob(() =>
		{
			if (Far.Api.Window.Kind == WindowKind.Panels)
				_ = StartAsync();
		});
	}

	private void OnKeyPressed(object? sender, KeyPressedEventArgs e)
	{
		switch (e.Key.VirtualKeyCode)
		{
			//! like WT
			case KeyCode.PageUp when e.Key.IsCtrlShift():
			case KeyCode.PageDown when e.Key.IsCtrlShift():
			case KeyCode.UpArrow when e.Key.IsCtrlShift():
			case KeyCode.DownArrow when e.Key.IsCtrlShift():
			case KeyCode.Home when e.Key.IsCtrlShift():
			case KeyCode.End when e.Key.IsCtrlShift():
				e.Ignore = true;
				ReadCommandForm.DoScroll(e);
				return;

			case KeyCode.Escape when e.Key.Is():
				// clear line or avoid closing
				if (_form.Edit.Line.Length > 0)
				{
					// clear text
					e.Ignore = true;
					_form.Edit.Text = string.Empty;
				}
				else if (__isConsoleMode)
				{
					// avoid closing
					e.Ignore = true;
				}
				return;

			case KeyCode.Spacebar when _form.Edit.Line.Length == 0 && !__isConsoleMode:
				// exit console on empty line
				e.Ignore = true;
				_form.Close();
				return;

			case KeyCode.Tab when e.Key.Is():
				// complete code
				e.Ignore = true;
				EditorKit.ExpandCode(_form.Edit.Line, null);
				return;

			case KeyCode.UpArrow when e.Key.Is():
			case KeyCode.E when e.Key.IsCtrl():
				// history navigation up
				e.Ignore = true;
				_form.Edit.Text = HistoryKit.GetNextCommand(true, _form.Edit.Text);
				return;

			case KeyCode.DownArrow when e.Key.Is():
			case KeyCode.X when e.Key.IsCtrl() && _form.Edit.Line.SelectionSpan.Length < 0:
				// history navigation down, mind selected ~ CtrlX Cut
				e.Ignore = true;
				_form.Edit.Text = HistoryKit.GetNextCommand(false, _form.Edit.Text);
				return;

			case KeyCode.F1 when e.Key.Is():
				// show help
				e.Ignore = true;
				Help.ShowHelpForText(_form.Edit.Text, _form.Edit.Line.Caret, HelpTopic.CommandConsole);
				return;

			case KeyCode.F2 when e.Key.Is():
				// user menu
				e.Ignore = true;
				Far.Api.PostMacro("mf.usermenu(0)");
				return;

			case KeyCode.F4 when e.Key.Is():
				// modal edit script
				e.Ignore = true;
				_form.DoEditor();
				return;

			case KeyCode.Enter when e.Key.IsCtrl():
				// insert current file name
				if (Far.Api.HasPanels)
				{
					e.Ignore = true;
					if (Far.Api.Panel!.CurrentFile is { } file)
						_form.Edit.Line.InsertText(file.Name);
				}
				return;

			case KeyCode.F when e.Key.IsCtrl() && Far.Api.HasPanels:
				// insert current file path
				if (Far.Api.HasPanels)
				{
					e.Ignore = true;
					if (Far.Api.Panel!.CurrentFile is { } file)
						_form.Edit.Line.InsertText(Path.Combine(Far.Api.CurrentDirectory, file.Name));
				}
				return;
		}
	}

	private static async Task DoFinallyAsync(WindowKind area1, int visiblePanels)
	{
		// close running
		if (Instance is { })
		{
			await Tasks.Job(() =>
			{
				Instance._form.Close();

				// with no panels save user screen printed during this REPL
				if (!Far.Api.HasPanels && Far.Api.Window.Kind == WindowKind.Desktop)
					Far.Api.UI.SaveUserScreen();
			});
			Instance = null;
		}

		// set panels
		var area2 = Far.Api.Window.Kind;
		if (area2 != WindowKind.Panels && Far.Api.HasPanels)
		{
			try
			{
				await Tasks.Job(() => Far.Api.Window.SetCurrentAt(-1));
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(ErrorCannotSetPanels, ex);
			}
		}

		// show panels
		if (visiblePanels > 0)
		{
			int visiblePanels2 = Far.Api.Window.CountVisiblePanels();
			if (visiblePanels != visiblePanels2)
			{
				if (visiblePanels == 2 && visiblePanels2 == 0)
				{
					await Tasks.Macro("Keys'CtrlO'");

					int vp = 0;
					for (int i = 0; i < 5; i++)
					{
						vp = Far.Api.Window.CountVisiblePanels();
						if (vp > 0)
							break;

						await Task.Delay(100);
						await Tasks.Macro("Keys'CtrlO'");
					}
					if (vp == 0)
						throw new InvalidOperationException("Cannot show panels.");
				}
				else if (visiblePanels == 2 && visiblePanels2 == 1)
				{
					if (Far.Api.Panel!.IsLeft)
						await Tasks.Macro("Keys'CtrlF2'");
					else
						await Tasks.Macro("Keys'CtrlF1'");
				}
				else if (visiblePanels == 1 && visiblePanels2 == 0)
				{
					if (Far.Api.Panel!.IsLeft)
						await Tasks.Macro("Keys'CtrlF1'");
					else
						await Tasks.Macro("Keys'CtrlF2'");
				}
			}
		}

		if (area2 != area1)
		{
			try
			{
				await Tasks.Job(() => Far.Api.Window.SetCurrentAt(Far.Api.Window.Count - 2));
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Cannot set area '{area1}'.", ex);
			}
		}

		// last, post quit
		if (__isConsoleMode)
		{
			if (Far.Api.Window.Count <= 2)
				Far.Api.PostJob(Far.Api.Quit);
		}
	}

	public static async Task StartAsync()
	{
		if (Instance is { })
		{
			Instance._form.Dialog.Activate();
			return;
		}

		try
		{
			// sync part

			var area1 = Far.Api.Window.Kind;
			if (area1 != WindowKind.Panels && Far.Api.HasPanels)
			{
				try
				{
					Far.Api.Window.SetCurrentAt(-1);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException(ErrorCannotSetPanels, ex);
				}
			}

			// hide panels
			int visiblePanels = Far.Api.Window.CountVisiblePanels();
			if (visiblePanels > 0)
				await Tasks.Macro("Keys'CtrlO'");

			A.Invoking();

			// async part

			// REPL
			try
			{
				for (bool run = true; run;)
				{
					// read
					Instance = await Tasks.Job(() => new ReadCommand());
					var args = await Instance._form.ReadAsync();
					if (args is null)
						return;

					// run
					Debug.WriteLine($"## CC: {args.Code}");

					var newPanel = await Tasks.Command(() => A.Run(args));
					if (newPanel is { } && !__isConsoleMode)
					{
						Stop();
						run = false;
					}
					else
					{
						A.SyncPathsBack();
					}

					if (A.FAR_PWSF_RUN)
						A.RunDone(args.Reason);
				}
			}
			finally
			{
				await DoFinallyAsync(area1, visiblePanels);
			}
		}
		catch (Exception ex)
		{
			_ = Tasks.Job(() => Far.Api.ShowError(Res.TextCommandConsole, ex));
		}
	}
}
