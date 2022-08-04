
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using System;
using System.Xml.Serialization;

namespace JavaScriptFar;

public class SessionConfiguration
{
	/// <summary>
	/// Defines document access options.
	/// </summary>
	/// <remarks>
	/// https://microsoft.github.io/ClearScript/Reference/html/T_Microsoft_ClearScript_DocumentAccessFlags.htm
	/// </remarks>
	[XmlIgnore] public DocumentAccessFlags DocumentAccessFlags { get; set; } = DocumentAccessFlags.EnableFileLoading;
	[XmlElement(nameof(DocumentAccessFlags))] public string DocumentAccessFlagsAsString { get => DocumentAccessFlags.ToString(); set => DocumentAccessFlags = Enum.Parse<DocumentAccessFlags>(value); }

	/// <summary>
	/// Semicolon-delimited list of directory URLs or paths to search for documents.
	/// Environment variables are expanded.
	/// </summary>
	/// <remarks>
	/// https://microsoft.github.io/ClearScript/Reference/html/P_Microsoft_ClearScript_DocumentSettings_SearchPath.htm
	/// </remarks>
	public string DocumentSearchPath { get; set; } = string.Empty;

	/// <summary>
	/// Defines options for initializing a new V8 JavaScript engine instance.
	/// </summary>
	/// <remarks>
	/// https://microsoft.github.io/ClearScript/Reference/html/T_Microsoft_ClearScript_V8_V8ScriptEngineFlags.htm
	/// </remarks>
	[XmlIgnore] public V8ScriptEngineFlags V8ScriptEngineFlags { get; set; }
	[XmlElement(nameof(V8ScriptEngineFlags))] public string V8ScriptEngineFlagsAsString { get => V8ScriptEngineFlags.ToString(); set => V8ScriptEngineFlags = Enum.Parse<V8ScriptEngineFlags>(value); }
}
