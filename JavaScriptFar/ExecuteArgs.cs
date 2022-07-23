
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace JavaScriptFar;

static partial class Actor
{
	internal class ExecuteArgs
	{
		public ExecuteArgs(string command)
		{
			Command = command;
		}

		public string Command { get; }
		public bool IsTask { get; set; }
		public bool IsDebug { get; set; }
		public bool IsDocument { get; set; }
		public Action<string> Print { get; set; }
	}
}
