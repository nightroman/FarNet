
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Management.Automation;

namespace PowerShellFar.Commands;

sealed class RegisterFarCommandCommand : BaseRegisterActionCmdlet
{
	[Parameter(Position = 2, Mandatory = true)]
	public EventHandler<ModuleCommandEventArgs> Handler { get; set; } = null!;

	[Parameter(Mandatory = true)]
	public string Prefix { get; set; } = null!;

	protected override void BeginProcessing()
	{
		base.BeginProcessing();

		A.Psf.Manager.RegisterCommand(
			new() { Id = Id.ToString(), Name = Name, Prefix = Prefix },
			Handler);
	}
}
