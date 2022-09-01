
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace PowerShellFar;

class RunArgs
{
	public string Code { get; private set; }

	public OutputWriter Writer { get; set; }

	public bool NoOutReason { get; set; }

	public bool UseLocalScope { get; set; }

	public object[] Arguments { get; set; }

	public Exception Reason { get; set; }

	public RunArgs(string code)
	{
		Code = code;
	}
}
