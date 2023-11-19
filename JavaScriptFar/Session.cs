
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JavaScriptFar;

sealed class Session : IDisposable
{
	public const string
		SessionFileMask = "_session.*",
		SessionConfigFile = "_session.xml",
		SessionParameterName = "_session";

	static readonly LinkedList<Session> s_sessions = new();

	public static IEnumerable<Session> Sessions => s_sessions;
	public string Root { get; }
	readonly ScriptEngine _engine;
	readonly SessionConfiguration _config;
	readonly DocumentCategory _documentCategory;

	public bool IsDebug => _config.V8ScriptEngineFlags.HasFlag(V8ScriptEngineFlags.EnableDebugging);

	private Session(string root, string? xml, IEnumerable<string> scripts, bool isDebug)
	{
		Root = root;

		_config = xml is null ? new() : new ModuleSettings<SessionConfiguration>(xml).GetData();
		if (isDebug)
			_config.V8ScriptEngineFlags |= V8ScriptEngineFlags.EnableDebugging;

		if (_config.V8ScriptEngineFlags.HasFlag(V8ScriptEngineFlags.EnableDebugging))
			EnsureNoDebugging();

		_documentCategory = _config.DocumentCategory switch
		{
			DefaultDocumentCategory.Script => DocumentCategory.Script,
			DefaultDocumentCategory.Standard => ModuleCategory.Standard,
			DefaultDocumentCategory.CommonJS => ModuleCategory.CommonJS,
			_ => throw new Exception()
		};

		_engine = V8ScriptEngine(root, _config);

		try
		{
			foreach (var document in scripts)
			{
				DocumentCategory category;
				if (IsFileScript(document))
					category = DocumentCategory.Script;
				else if (IsFileCommonJS(document))
					category = ModuleCategory.CommonJS;
				else if (IsFileStandard(document))
					category = ModuleCategory.Standard;
				else
					continue;

				_engine.Execute(new DocumentInfo(new Uri(document)) { Category = category }, File.ReadAllText(document));
			}
		}
		catch (ScriptEngineException ex)
		{
			_engine.Dispose();
			throw ModuleExceptionFromScriptEngineException(ex);
		}
	}

	// see ClearScriptConsole.cs
	static V8ScriptEngine V8ScriptEngine(string root, SessionConfiguration config)
	{
		var engine = new V8ScriptEngine(root, config.V8ScriptEngineFlags)
		{
			AllowReflection = true,
			SuppressExtensionMethodEnumeration = true
		};

		engine.DocumentSettings.AccessFlags = config.DocumentAccessFlags;

		if (!string.IsNullOrEmpty(config.DocumentSearchPath))
			engine.DocumentSettings.SearchPath = Environment.ExpandEnvironmentVariables(config.DocumentSearchPath);

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

	public void Dispose()
	{
		s_sessions.Remove(this);
		_engine.Dispose();
	}

	public static bool IsFileScript(string path) => path.EndsWith(".js", StringComparison.OrdinalIgnoreCase);
	public static bool IsFileCommonJS(string path) => path.EndsWith(".cjs", StringComparison.OrdinalIgnoreCase);
	public static bool IsFileStandard(string path) => path.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase);
	public static bool IsFileDocument(string path) => IsFileScript(path) || IsFileCommonJS(path) || IsFileStandard(path);

	public static ModuleException ModuleExceptionFromScriptEngineException(Exception ex)
	{
		var message = ex.Message;

		if (ex is ScriptEngineException seex)
			if (seex.ErrorDetails != null && seex.ErrorDetails.StartsWith(message))
				message = seex.ErrorDetails;

		return new ModuleException(message, ex);
	}

	static void EnsureNoDebugging()
	{
		while (s_sessions.FirstOrDefault(x => x.IsDebug) is Session session)
			session.Dispose();
	}

	void SetHostArgs(IDictionary? parameters)
	{
		var bag = new PropertyBag();
		if (parameters is not null)
		{
			foreach (DictionaryEntry kv in parameters)
			{
				//! pass the original Value, with Interop it may be any
				bag.Add(kv.Key.ToString(), kv.Value);
			}
		}
		_engine.AddHostObject("args", bag);
	}

	static (string, string[]) GetSessionRootAndFiles(ExecuteArgs args)
	{
		bool isRootFromParameter = args.Parameters is not null && args.Parameters.Contains(SessionParameterName);

		string root;
		if (isRootFromParameter)
		{
			root = args.Parameters![SessionParameterName] as string ?? throw new ModuleException("Parameter _session must be string.");

			if (!Path.IsPathRooted(root))
				root = Path.Join(Far.Api.CurrentDirectory, root);

			root = Path.GetFullPath(root);
		}
		else
		{
			root = args.Document is not null ? Path.GetDirectoryName(args.Document)! : Far.Api.CurrentDirectory;
		}

		var files = Directory.GetFiles(root, SessionFileMask);
		if (files.Length == 0 && !isRootFromParameter)
		{
			root = JavaScriptModule.Root;
			files = Directory.GetFiles(root, SessionFileMask);
		}

		Array.Sort(files, StringComparer.OrdinalIgnoreCase);
		return (root, files);
	}

	public static Session GetOrCreateSession(ExecuteArgs args)
	{
		var (root, files) = GetSessionRootAndFiles(args);

		var session = s_sessions.FirstOrDefault(x => string.Equals(x.Root, root, StringComparison.OrdinalIgnoreCase));
		if (session is not null)
		{
			// the session exists but not debugged
			if (args.IsDebug && !session.IsDebug)
			{
				session.Dispose();
				session = null;
			}
			else
			{
				// update parameters
				session.SetHostArgs(args.Parameters!);

				// and make it first in the list
				if (s_sessions.First!.Value != session)
				{
					s_sessions.Remove(session);
					s_sessions.AddFirst(session);
				}

				return session;
			}
		}

		var xml = files.FirstOrDefault(x => x.EndsWith(SessionConfigFile, StringComparison.OrdinalIgnoreCase));

		session = new Session(root, xml, files, args.IsDebug);
		session.SetHostArgs(args.Parameters);
		s_sessions.AddFirst(session);

		return session;
	}

	(DocumentInfo, string) GetDocumentAndCode(string document)
	{
		DocumentCategory category;
		if (IsFileCommonJS(document))
			category = ModuleCategory.CommonJS;
		else if (IsFileStandard(document))
			category = ModuleCategory.Standard;
		else
			category = _documentCategory;

		var doc = new DocumentInfo(new Uri(document)) { Category = category };
		var code = File.ReadAllText(document);
		return (doc, code);
	}

	public object EvaluateDocument(string document)
	{
		var (doc, code) = GetDocumentAndCode(document);
		return _engine.Evaluate(doc, code);
	}

	public void ExecuteDocument(string document)
	{
		var (doc, code) = GetDocumentAndCode(document);
		_engine.Execute(doc, code);
	}

	public object EvaluateCommand(string code)
	{
		return _engine.Evaluate(code);
	}

	public string ExecuteCommand(string code)
	{
		return _engine.ExecuteCommand(code);
	}
}
