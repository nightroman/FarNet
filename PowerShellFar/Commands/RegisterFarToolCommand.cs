
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Management.Automation;

namespace PowerShellFar.Commands;

sealed class RegisterFarToolCommand : BaseRegisterActionCmdlet
{
	[Parameter(Position = 2, Mandatory = true)]
	public EventHandler<ModuleToolEventArgs> Handler { get; set; } = null!;

	[Parameter(Mandatory = true)]
	public ModuleToolOptions Options { get; set; }

	protected override void BeginProcessing()
	{
		base.BeginProcessing();

		A.Psf.Manager.RegisterTool(
			new() { Id = Id.ToString(), Name = Name, Options = Options },
			Handler);
	}
}
