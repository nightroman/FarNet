
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Management.Automation;

namespace PowerShellFar.Commands;

/// <summary>
/// Common features of register cmdlets.
/// </summary>
abstract class BaseRegisterActionCmdlet : BaseCmdlet
{
	[Parameter(Position = 0, Mandatory = true)]
	public string Name { get; set; } = null!;

	[Parameter(Position = 1, Mandatory = true)]
	public Guid Id { get; set; }

	protected override void BeginProcessing()
	{
		if (Far.Api.GetModuleAction(Id) is { } action)
			action.Unregister();
	}
}
