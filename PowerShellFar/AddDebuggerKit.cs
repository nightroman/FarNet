
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.Management.Automation;

namespace PowerShellFar;

static class AddDebuggerKit
{
	// just adds the debugger
	const string CodeAddDebugger = @"
param($Parameters)
Add-Debugger.ps1 @Parameters
";

	public static void ValidateAvailable()
	{
		if (0 == A.InvokeCode("Get-Command Add-Debugger.ps1 -Type ExternalScript -ErrorAction 0").Count)
			throw new InvalidOperationException(
				"Cannot find the required script Add-Debugger.ps1.\nInstall from PSGallery -- https://www.powershellgallery.com/packages/Add-Debugger");
	}

	public static void AddDebugger(PowerShell ps, IDictionary parameters)
	{
		ps.AddScript(CodeAddDebugger).AddArgument(parameters).Invoke();
		ps.Commands.Clear();
	}
}
