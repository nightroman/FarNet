
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;

namespace JavaScriptFar;

class ExecuteArgs
{
	public string Command { get; set; }
	public string Document { get; set; }
	public bool IsTask { get; set; }
	public bool IsDebug { get; set; }
	public Action<string> Print { get; set; }
	public IDictionary Parameters { get; set; }
}
