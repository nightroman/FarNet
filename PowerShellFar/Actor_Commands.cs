
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PowerShellFar
{
	public sealed partial class Actor
	{
		static UI.InputDialog CreateCodeDialog()
		{
			return new UI.InputDialog() { Title = Res.Me, History = Res.History, UseLastHistory = true, Prompt = new string[] { Res.InvokeCommands } };
		}

		/// <summary>
		/// Shows an input dialog and returns entered PowerShell code.
		/// </summary>
		/// <remarks>
		/// It is called by the plugin menu command "Invoke commands". You may call it, too.
		/// It is just an input box for any text but it is designed for PowerShell code input,
		/// e.g. TabExpansion is enabled (by [Tab]).
		/// <para>
		/// The code is simply returned, if you want to execute it then call <see cref="InvokeInputCode"/>.
		/// </para>
		/// </remarks>
		public string InputCode()
		{
			var ui = CreateCodeDialog();
			return ui.Show();
		}

		/// <summary>
		/// Prompts to input code and invokes it.
		/// </summary>
		public void InvokeInputCode()
		{
			InvokeInputCodePrivate(null);
		}

		void InvokeInputCodePrivate(string input)
		{
			var ui = CreateCodeDialog();
			ui.Text = input;
			var code = ui.Show();
			if (!string.IsNullOrEmpty(code))
				Run(new RunArgs(code) { AddHistory = Far.Api.MacroState == MacroState.None });
		}

		/// <summary>
		/// Invokes the selected text or the current line text in the editor or the command line.
		/// Called on "Invoke selected".
		/// </summary>
		public void InvokeSelectedCode()
		{
			EditorKit.InvokeSelectedCode();
		}

		/// <summary>
		/// Prompts for PowerShell commands.
		/// Called on "Invoke commands".
		/// </summary>
		public async void StartInvokeCommands()
		{
			if (Far.Api.Window.Kind == WindowKind.Panels)
			{
				StartCommandConsole();
			}
			else
			{
				var ui = CreateCodeDialog();
				var code = await ui.ShowAsync();
				if (!string.IsNullOrEmpty(code))
					await Tasks.Job(() => Run(new RunArgs(code) { AddHistory = Far.Api.MacroState == MacroState.None }));
			}
		}

		string GetCommandPrompt()
		{
			try
			{
				SyncPaths();

				using var ps = NewPowerShell();
				var res = ps.AddCommand("prompt").Invoke();

				//! as PS, use not empty res[0]
				string prompt;
				if (res.Count > 0 && res[0] != null && (prompt = res[0].ToString()).Length > 0)
					return prompt;
			}
			catch (RuntimeException)
			{
			}

			return "PS> ";
		}

		/// <summary>
		/// Stops "Command console".
		/// </summary>
		public void StopCommandConsole()
		{
			if (UI.ReadCommand.Instance != null)
				UI.ReadCommand.Instance.Stop();
		}

		/// <summary>
		/// Starts "Command console" and waits for it.
		/// </summary>
		/// <returns></returns>
		public Task RunCommandConsole()
		{
			StartCommandConsole();
			return FarNet.Works.Tasks2.Wait(nameof(StartCommandConsole), () =>
				Far.Api.Window.Kind == WindowKind.Dialog &&
				Far.Api.Dialog.TypeId == new Guid(Guids.ReadCommandDialog));
		}

		/// <summary>
		/// Starts "Command console".
		/// </summary>
		public async void StartCommandConsole()
		{
			try
			{
				// already started? activate
				if (UI.ReadCommand.Instance != null)
				{
					await UI.ReadCommand.Instance.ActivateAsync();
					return;
				}

				// must be panels
				if (Far.Api.Window.Kind != WindowKind.Panels)
					throw new ModuleException("Command console should start from panels.");

				// hide key bar (hack)
				bool visibleKeyBar = Console.CursorTop - Console.WindowTop == Console.WindowHeight - 2;
				if (visibleKeyBar)
					await Tasks.Macro("Keys'CtrlB'");

				try
				{
					// REPL
					for (; ; )
					{
						// prompt
						var args = new UI.ReadCommand.Args { GetPrompt = GetCommandPrompt };

						// read
						UI.ReadCommand.Instance = await Tasks.Job(() => new UI.ReadCommand(args));
						var res = await UI.ReadCommand.Instance.ReadAsync();
						if (res == null)
							return;

						// run
						var obj = await Tasks.Command(() => Run(res));

						// switch to opened panel, come back on exit
						if (obj is Panel panel)
						{
							panel.Closed += (_, _) => StartCommandConsole();
							return;
						}
					}
				}
				finally
				{
					UI.ReadCommand.Instance = null;

					// restore key bar (may not work with jobs)
					if (visibleKeyBar)
						await Tasks.Macro("Keys'CtrlB'");
				}
			}
			catch (Exception ex)
			{
				_ = Tasks.Job(() => Far.Api.ShowError(nameof(StartCommandConsole), ex));
			}
		}
	}
}
