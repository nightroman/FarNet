
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Management.Automation;

namespace PowerShellFar.Commands;

sealed class RegisterFarDrawerCommand : BaseRegisterActionCmdlet
{
	[Parameter(Position = 2, Mandatory = true)]
	public Action<IEditor, ModuleDrawerEventArgs> Handler { get; set; } = null!;

	[Parameter(Mandatory = true)]
	public string Mask { get; set; } = null!;

	[Parameter]
	public int Priority { get; set; }

	protected override void BeginProcessing()
	{
		base.BeginProcessing();

		A.Psf.Manager.RegisterDrawer(
			new() { Id = Id.ToString(), Name = Name, Mask = Mask, Priority = Priority },
			Handler);
	}
}
