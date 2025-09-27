using FarNet;
using Microsoft.PowerShell;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellFar;

internal static class FarInitialSessionState
{
	public static InitialSessionState Instance = Create();

	private static InitialSessionState Create()
	{
		// initial state
		var state = InitialSessionState.CreateDefault();
		state.ExecutionPolicy = ExecutionPolicy.Bypass;

		// add cmdlets
		Commands.BaseCmdlet.AddCmdlets(state);

		// add variables
		state.Variables.Add([
			new("Far", Far.Api, "Exposes FarNet.", ScopedItemOptions.AllScope | ScopedItemOptions.Constant),
			new("Psf", A.Psf, "Exposes PowerShellFar.", ScopedItemOptions.AllScope | ScopedItemOptions.Constant),
			new("ErrorActionPreference", ActionPreference.Stop, string.Empty)
		]);

		return state;
	}
}
