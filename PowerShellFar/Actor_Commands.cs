
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
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

		/// <summary>
		/// Starts "Command console".
		/// </summary>
		public void StartCommandConsole()
		{
			_ = UI.ReadCommand.StartAsync();
		}

		/// <summary>
		/// Stops "Command console".
		/// </summary>
		public void StopCommandConsole()
		{
			UI.ReadCommand.Stop();
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

	}
}
