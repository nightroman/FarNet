
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JavaScriptFar;

sealed class Session : IDisposable
{
	const string SessionScriptName = "_session.js";

	static readonly LinkedList<Session> s_sessions = new();

	public static IEnumerable<Session> Sessions => s_sessions;
	public string Root { get; }
	public bool IsDebug { get; }
	ScriptEngine _engine { get; }

	private Session(string root, string script, bool isDebug)
	{
		Root = root;
		IsDebug = isDebug;
		_engine = V8ScriptEngine(isDebug);

		if (script is not null)
		{
			try
			{
				_engine.Execute(new DocumentInfo(new Uri(script)), File.ReadAllText(script));
			}
			catch (ScriptEngineException ex)
			{
				throw ModuleExceptionFromScriptEngineException(ex);
			}
		}
	}

	public void Dispose()
	{
		s_sessions.Remove(this);
		_engine.Dispose();
	}

	public static ModuleException ModuleExceptionFromScriptEngineException(Exception ex)
	{
		var message = ex.Message;

		if (ex is ScriptEngineException seex)
			if (seex.ErrorDetails != null && seex.ErrorDetails.StartsWith(message))
				message = seex.ErrorDetails;

		return new ModuleException(message, ex);
	}

	// see ClearScriptConsole.cs
	static ScriptEngine V8ScriptEngine(bool isDebug)
	{
		var flags = isDebug ? V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart : V8ScriptEngineFlags.None;
		var engine = new V8ScriptEngine(Res.MyName, flags)
		{
			AllowReflection = true,
			SuppressExtensionMethodEnumeration = true
		};

		engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

		engine.AddHostObject("host", new ExtendedHostFunctions());
		engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection(
			"mscorlib",
			"System",
			"System.Core",
			"System.Diagnostics.Process",
			"System.Numerics",
			"System.Runtime",
			"ClearScript.Core",
			"FarNet"
		));

		// far object
		engine.AddHostObject("far", Far.Api);

		return engine;
	}

	public static Session GetOrCreateSession(ExecuteArgs args)
	{
		var root = args.IsDocument ? Path.GetDirectoryName(args.Command) : Far.Api.CurrentDirectory;
		var script = Path.Combine(root, SessionScriptName);
		if (!File.Exists(script))
		{
			root = JavaScriptModule.Root;
			script = Path.Combine(root, SessionScriptName);
			if (!File.Exists(script))
				script = null;
		}

		var session = s_sessions.FirstOrDefault(x => string.Equals(x.Root, root, StringComparison.OrdinalIgnoreCase));
		if (session is not null)
		{
			if (args.IsDebug && !session.IsDebug)
			{
				session.Dispose();
			}
			else
			{
				if (s_sessions.First.Value != session)
				{
					s_sessions.Remove(session);
					s_sessions.AddFirst(session);
				}

				return session;
			}
		}

		session = new Session(root, script, args.IsDebug);
		s_sessions.AddFirst(session);
		return session;
	}

	public object Evaluate(DocumentInfo documentInfo, string code)
	{
		return _engine.Evaluate(documentInfo, code);
	}

	public void Execute(DocumentInfo documentInfo, string code)
	{
		_engine.Execute(documentInfo, code);
	}

	public string ExecuteCommand(string code)
	{
		return _engine.ExecuteCommand(code);
	}
}
