
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections;

namespace JavaScriptFar;

public class JavaScriptModule : ModuleHost
{
	public static JavaScriptModule Instance { get; private set; }
	public static string Root { get; private set; }

	public JavaScriptModule()
	{
		Instance = this;
		Root = Manager.GetFolderPath(SpecialFolder.RoamingData, true);
	}

	public override object Interop(string command, object args)
	{
		return command switch
		{
			"EvaluateCommand" => new Func<string, IDictionary, object>((string command, IDictionary parameters) =>
			{
				var args = new ExecuteArgs { Command = command, Parameters = parameters };
				var session = Session.GetOrCreateSession(args);
				return session.EvaluateCommand(command);
			}),
			"EvaluateDocument" => new Func<string, IDictionary, object>((string document, IDictionary parameters) =>
			{
				var args = new ExecuteArgs { Document = document, Parameters = parameters };
				var session = Session.GetOrCreateSession(args);
				return session.EvaluateDocument(document);
			}),
			"StartDebugging" => new Action<string>((string root) =>
			{
				var args = new ExecuteArgs { IsDebug = true, Parameters = new Hashtable() { { "_session", root } } };
				Session.GetOrCreateSession(args);
			}),
			_ => throw new ArgumentException("Unknown command.", nameof(command)),
		};
	}
}
