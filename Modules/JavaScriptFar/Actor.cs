using FarNet;
using Microsoft.ClearScript;
using System;
using System.IO;

namespace JavaScriptFar;

static class Actor
{
	static ScriptEngine _session;

	static ScriptEngine CreateScriptEngine()
	{
		// see ClearScriptConsole.cs
		var engine = ScriptEngines.V8ScriptEngine();
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
		public bool IsDocument { get; set; }
		public Action<string> Print { get; set; }
	}

	internal static void Execute(ExecuteArgs args)
	{
		ScriptEngine engine;
		if (args.IsDocument)
		{
			engine = CreateScriptEngine();
		}
		else
		{
			if (_session is null)
				_session = CreateScriptEngine();

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

						// [void] in V8, [undefined] in JScript
						if (text != "[void]" && text != "[undefined]")
							args.Print(text);
					}
				}
			}
			else
			{
				var res = engine.ExecuteCommand(args.Command);
				if (args.Print is not null)
				{
					args.Print(res);
				}
				else
				{
					Far.Api.UI.ShowUserScreen();
					Far.Api.UI.WriteLine($"{JavaScriptCommand.Prefix}: {args.Command}", ConsoleColor.DarkGray);
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
			if (args.IsDocument)
				engine.Dispose();
		}
	}
}
