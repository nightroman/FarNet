using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

/// <summary>
/// Opening text files.
/// </summary>
class BaseTextCmdlet : BaseCmdlet
{
	protected const string PsnMain = "Main";

	[Parameter(ParameterSetName = PsnMain)]
	public string? Title { get; set; }

	[Parameter(ParameterSetName = PsnMain)]
	public DeleteSource DeleteSource { get; set; }

	[Parameter(ParameterSetName = PsnMain)]
	public SwitchParameter DisableHistory { get; set; }

	[Parameter(ParameterSetName = PsnMain)]
	public Switching Switching { get; set; }

	[Parameter(ParameterSetName = PsnMain)]
	public int CodePage { get; set; } = -1;
}
