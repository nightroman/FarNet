
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

/// <summary>
/// Common features of cmdlets opening text files.
/// </summary>
class BaseTextCmdlet : BaseCmdlet
{
	[Parameter]
	public string? Title { get; set; }

	[Parameter]
	public DeleteSource DeleteSource { get; set; }

	[Parameter]
	public SwitchParameter DisableHistory { get; set; }

	[Parameter]
	public Switching Switching { get; set; }

	[Parameter]
	public int CodePage { get; set; } = -1;
}
