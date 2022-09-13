
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

sealed class OpenFarPanelCommand : BasePanelCmdlet
{
	Panel? _Panel;

	[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
	public PSObject InputObject { get; set; } = null!;

	[Parameter]
	public SwitchParameter AsChild { get; set; }

	protected override void ProcessRecord()
	{
		// ignore empty or the rest of input
		if (InputObject is null || _Panel != null)
			return;

		// get the panel or a new member panel
		if (!(InputObject.BaseObject is Explorer explorer))
			_Panel = (InputObject.BaseObject as Panel) ?? new MemberPanel(new MemberExplorer(InputObject));
		else
			_Panel = explorer.CreatePanel();

		// setup and show
		ApplyParameters(_Panel);
		if (AsChild)
			_Panel.OpenChild(null);
		else
			_Panel.Open();
	}
}
