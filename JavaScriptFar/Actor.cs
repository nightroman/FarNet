using FarNet;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using System;
using System.Diagnostics;
using System.IO;

namespace JavaScriptFar;

static class Actor
{
	static ScriptEngine s_engine;

	static ScriptEngine CreateScriptEngine(bool isDebug)
	{
		var engine = ScriptEngines.V8ScriptEngine(isDebug);
		engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

		// see ClearScriptConsole.cs
		engine.AddHostObject("host", new ExtendedHostFunctions());
		engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection(
			"mscorlib",
			"System",
			"System.Core",
			"System.Numerics",
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
		string debugWindowTitle = null;
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
			debugWindowTitle = Far.Api.UI.WindowTitle;
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
			if (s_engine is null)
				s_engine = CreateScriptEngine(false);

			engine = s_engine;
		}

		try
		{
			if (args.IsDocument)
			{
				var doc = new DocumentInfo(new Uri(args.Command)) { Category = ModuleCategory.Standard };
				var code = File.ReadAllText(args.Command);

				if (args.Print is null)
				{
					engine.Execute(doc, code);
				}
				else
				{
					var res = engine.Evaluate(doc, code);
					if (res is not null)
						args.Print(res.ToString());
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
		catch (ScriptEngineException ex)
		{
			var message = ex.Message;
			if (ex.ErrorDetails != null && ex.ErrorDetails.StartsWith(message))
				message = ex.ErrorDetails;

			throw new ModuleException(message, ex);
		}
		finally
		{
			// restore title and progress
			if (args.IsDebug && debugWindowTitle is not null)
			{
				Far.Api.UI.WindowTitle = debugWindowTitle;
				Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
			}

			if (args.IsDocument)
				engine.Dispose();
		}
	}
}
