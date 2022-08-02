
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace JavaScriptFar;

static class Actor
{
	static void StartExecuteTask(Session session, DocumentInfo doc, string code, ExecuteArgs args)
	{
		Task.Run(() =>
		{
			try
			{
				session.Execute(doc, code);
			}
			catch (Exception ex)
			{
				Tasks.Job(() => Far.Api.ShowError(
					"JavaScript task error",
					Session.ModuleExceptionFromScriptEngineException(ex)));
			}
		});
	}

	public static void Execute(ExecuteArgs args)
	{
		string windowTitle = null;
		if (args.IsDebug)
		{
			if (0 != Far.Api.Message("Click OK to open VSCode and manually start ClearScript V8 debugger.", Res.DebugTitle, MessageOptions.OkCancel))
				return;

			try
			{
				Process.Start(new ProcessStartInfo("code.cmd") { Arguments = "--reuse-window", UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden });
			}
			catch (Exception ex)
			{
				throw new ModuleException($"Cannot start code.cmd: {ex.Message}", ex);
			}

			// set title and progress
			if (!args.IsTask)
			{
				windowTitle = Far.Api.UI.WindowTitle;
				Far.Api.UI.WindowTitle = Res.DebugTitle;
				Far.Api.UI.SetProgressState(TaskbarProgressBarState.Paused);
			}
		}

		var session = Session.GetOrCreateSession(args);
		try
		{
			if (args.IsDocument)
			{
				var doc = new DocumentInfo(new Uri(args.Command)) { Category = ModuleCategory.Standard };
				var code = File.ReadAllText(args.Command);

				if (args.IsTask)
				{
					StartExecuteTask(session, doc, code, args);
				}
				else if (args.Print is null)
				{
					session.Execute(doc, code);
				}
				else
				{
					var res = session.Evaluate(doc, code);
					if (res is not null)
						args.Print(res.ToString());
				}
			}
			else
			{
				var code = args.Command;

				var res = session.ExecuteCommand(code);
				if (args.Print is not null)
				{
					args.Print(res);
				}
				else
				{
					Far.Api.UI.ShowUserScreen();
					Far.Api.UI.WriteLine($"{Res.Prefix}: {args.Command}", ConsoleColor.DarkGray);
					Far.Api.UI.WriteLine(res);
					Far.Api.UI.SaveUserScreen();
				}
			}
		}
		catch (ScriptEngineException ex)
		{
			throw Session.ModuleExceptionFromScriptEngineException(ex);
		}
		finally
		{
			// restore title and progress
			if (windowTitle is not null)
			{
				Far.Api.UI.WindowTitle = windowTitle;
				Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
			}
		}
	}
}
