
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

static partial class Actor
{
	static ScriptEngine s_engine;
	static bool s_isTaskDebug;

	static ScriptEngine CreateScriptEngine(ExecuteArgs args)
	{
		var engine = ScriptEngines.V8ScriptEngine(args.IsDebug);
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

	static ModuleException ModuleExceptionFromScriptEngineException(Exception ex)
	{
		var message = ex.Message;

		if (ex is ScriptEngineException seex)
		if (seex.ErrorDetails != null && seex.ErrorDetails.StartsWith(message))
			message = seex.ErrorDetails;

		return new ModuleException(message, ex);
	}

	static void StartExecuteTask(ScriptEngine engine, DocumentInfo doc, string code, ExecuteArgs args)
	{
		Task.Run(() =>
		{
			if (args.IsDebug)
				s_isTaskDebug = true;

			try
			{
				engine.Execute(doc, code);
			}
			catch (Exception ex)
			{
				Tasks.Job(() => Far.Api.ShowError(
					"JavaScript task error",
					ModuleExceptionFromScriptEngineException(ex)));
			}
			finally
			{
				if (args.IsDebug)
					s_isTaskDebug = false;

				engine.Dispose();
			}
		});
	}

	internal static void Execute(ExecuteArgs args)
	{
		string windowTitle = null;
		if (args.IsDebug)
		{
			if (args.IsTask && s_isTaskDebug)
				throw new ModuleException("Cannot debug two tasks.");

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

		ScriptEngine engine;
		if (args.IsDocument)
		{
			engine = CreateScriptEngine(args);
		}
		else
		{
			if (s_engine is null)
				s_engine = CreateScriptEngine(args);

			engine = s_engine;
		}

		try
		{
			if (args.IsDocument)
			{
				var doc = new DocumentInfo(new Uri(args.Command)) { Category = ModuleCategory.Standard };
				var code = File.ReadAllText(args.Command);

				if (args.IsTask)
				{
					StartExecuteTask(engine, doc, code, args);
					engine = null;
				}
				else if (args.Print is null)
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
			throw ModuleExceptionFromScriptEngineException(ex);
		}
		finally
		{
			// restore title and progress
			if (windowTitle is not null)
			{
				Far.Api.UI.WindowTitle = windowTitle;
				Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
			}

			// dispose temp engine
			if (engine is not null && engine != s_engine)
				engine.Dispose();
		}
	}
}
