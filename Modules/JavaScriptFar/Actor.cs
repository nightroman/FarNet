using FarNet;
using Microsoft.ClearScript;
using System;
using System.Diagnostics;
using System.IO;

namespace JavaScriptFar;

static class Actor
{
	static ScriptEngine _session;

	static ScriptEngine CreateScriptEngine(bool isDebug)
	{
		// see ClearScriptConsole.cs
		var engine = ScriptEngines.V8ScriptEngine(isDebug);
		engine.AddHostObject("host", new ExtendedHostFunctions());
		engine.AddHostObject("lib", HostItemFlags.GlobalMembers, new HostTypeCollection(
			"mscorlib",
			"System",
			"System.Core",
			"System.Numerics",
			"ClearScript.Core",
			"ClearScript.V8",
			"FarNet"
		));

		// far object
		engine.AddHostObject("far", Far.Api);

		return engine;
	}

	internal class ExecuteArgs
	{
		public ExecuteArgs(string command)
		{
			Command = command;
		}

		public string Command { get; }
		public bool IsDebug { get; set; }
		public bool IsDocument { get; set; }
		public Action<string> Print { get; set; }
	}

	internal static void Execute(ExecuteArgs args)
	{
		string windowTitle = null;
		if (args.IsDebug)
		{
			if (0 != Far.Api.Message("After clicking OK start the debugger in the opened VSCode.", Res.DebugTitle, MessageOptions.OkCancel))
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
			windowTitle = Far.Api.UI.WindowTitle;
			Far.Api.UI.WindowTitle = Res.DebugTitle;
			Far.Api.UI.SetProgressState(TaskbarProgressBarState.Paused);
		}

		ScriptEngine engine;
		if (args.IsDocument)
		{
			engine = CreateScriptEngine(args.IsDebug);
		}
		else
		{
			if (_session is null)
				_session = CreateScriptEngine(false);

			engine = _session;
		}

		try
		{
			if (args.IsDocument)
			{
				var doc = new DocumentInfo(new Uri(args.Command));
				var code = File.ReadAllText(args.Command);

				if (args.Print is null)
				{
					engine.Execute(doc, code);
				}
				else
				{
					var res = engine.Evaluate(doc, code);
					if (res is not null)
					{
						var text = res.ToString(); //rk-0 can res be not string? we might use own formatter

						// skip [void] in V8, [undefined] in JScript
						if (text != "[void]")
							args.Print(text);
					}
				}
			}
			else
			{
				var code = args.Command;

				var res = engine.ExecuteCommand(code);
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
		catch (ScriptEngineException ex) //rk-0 think dialog with goto
		{
			var message = ex.Message;
			if (ex.ErrorDetails != null && ex.ErrorDetails.StartsWith(message))
				message = ex.ErrorDetails;

			throw new ModuleException(message, ex);
		}
		finally
		{
			// restore title and progress
			if (args.IsDebug && windowTitle is not null)
			{
				Far.Api.UI.WindowTitle = windowTitle;
				Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
			}

			if (args.IsDocument)
				engine.Dispose();
		}
	}
}
