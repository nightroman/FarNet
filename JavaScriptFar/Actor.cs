
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using Microsoft.ClearScript;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace JavaScriptFar;

static class Actor
{
	static void StartVSCode(string arguments)
	{
		Process.Start(new ProcessStartInfo("code.cmd") { Arguments = arguments, UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden });
	}

	public static void StartDebugging(string document = null, int lineNumber = 1, int columnNumber = 1)
	{
		var args = new ExecuteArgs { IsDebug = true };

		try
		{
			if (document is null)
			{
				StartVSCode("--reuse-window");
			}
			else
			{
				args.Document = document;
				StartVSCode($"--goto \"{document}\":{lineNumber}:{columnNumber}");
			}
		}
		catch (Exception ex)
		{
			throw new ModuleException($"Cannot start code.cmd: {ex.Message}", ex);
		}

		Session.GetOrCreateSession(args);
	}

	static void StartExecuteTask(Session session, ExecuteArgs args)
	{
		Task.Run(() =>
		{
			try
			{
				if (args.Document is not null)
					session.ExecuteDocument(args.Document);
				else
					session.ExecuteCommand(args.Command);
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
		var session = Session.GetOrCreateSession(args);
		try
		{
			if (args.IsTask)
			{
				StartExecuteTask(session, args);
			}
			else if (args.Document is not null)
			{
				if (args.Print is null)
				{
					session.ExecuteDocument(args.Document);
				}
				else
				{
					var res = session.EvaluateDocument(args.Document);
					if (res is not null)
						args.Print(res.ToString());
				}
			}
			else
			{
				var res = session.ExecuteCommand(args.Command);
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
	}
}
