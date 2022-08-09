using FarNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace PowerShellFar;

using Write = Action<string>;

///
public class DebuggingRunner
{
	readonly DebuggerStopEventArgs _args;

	///
	public int Context { get; set; } = 5;


	internal DebuggingRunner(DebuggerStopEventArgs args)
	{
		_args = args;
	}

	void WriteDebuggerFile(string path, int lineIndex, int lineCount, int markIndex, Write write)
	{
		// amend negative start
		if (lineIndex < 0)
		{
			lineCount += lineIndex;
			lineIndex = 0;
		}

		// content lines
		var lines = File.ReadAllLines(path); //rk-0 need just a few

		int totalCount = Math.Min(lineIndex + lineCount, lines.Length);
		do
		{
			var mark = lineIndex == markIndex ? "=> " : "   ";
			write(mark + lines[lineIndex]);
		} while (++lineIndex < totalCount);
	}

	void WriteText(string text, Write write)
	{
		var lines = text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		foreach (var line in lines)
			write(line);
	}

	internal void WriteDebuggerInfo(InvocationInfo invocationInfo, int context, Write write)
	{
		// write position message
		WriteText(invocationInfo.PositionMessage.Trim(), write);
		write(string.Empty);

		// done?
		var file = invocationInfo.ScriptName;
		if (context < 0 || file == null || !File.Exists(file))
			return;

		// write file lines
		var markIndex = invocationInfo.ScriptLineNumber - 1;
		WriteDebuggerFile(file, markIndex - context, 2 * context + 1, markIndex, write);
	}

	///
	public string Info(int context)
	{
		if (context > 0)
			Context = context;
		else
			context = Context;

		var writer = new StringWriter();
		WriteDebuggerInfo(_args.InvocationInfo, context, writer.WriteLine);
		return writer.ToString();
	}

	///
	public void Continue() => _args.ResumeAction = DebuggerResumeAction.Continue;

	///
	public void StepInto() => _args.ResumeAction = DebuggerResumeAction.StepInto;

	///
	public void StepOut() => _args.ResumeAction = DebuggerResumeAction.StepOut;

	///
	public void StepOver() => _args.ResumeAction = DebuggerResumeAction.StepOver;

	///
	public void Stop() => _args.ResumeAction = DebuggerResumeAction.Stop;
}

static class Debugging
{
	static readonly Action<string> StartDebugging;
	static readonly Func<string, IDictionary, object> EvaluateCommand;

	static Debugging()
	{
		var manager = Far.Api.GetModuleManager("JavaScriptFar");
		if (manager == null)
			return;

		StartDebugging = (Action<string>)manager.Interop("StartDebugging", null);
		EvaluateCommand = (Func<string, IDictionary, object>)manager.Interop("EvaluateCommand", null);
	}

	public static bool CanDebug => StartDebugging is not null;

	public static void OnDebuggerStop(DebuggerStopEventArgs args)
	{
		var root = @"C:\ROM\FarDev\Code\JavaScriptFar\Debugging";

		StartDebugging(root);

		var runner = new DebuggingRunner(args);

		var writer = new StringWriter();
		runner.WriteDebuggerInfo(args.InvocationInfo, runner.Context, s => { writer.Write("// "); writer.WriteLine(s); });
		writer.WriteLine();
		writer.WriteLine("debugger;");
		var command = writer.ToString();

		EvaluateCommand(command, new Dictionary<string, object>() {
			{ "_session", root },
			{ "runner", runner }
		});
	}
}
