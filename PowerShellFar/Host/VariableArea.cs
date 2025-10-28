using FarNet;
using System.Management.Automation;

namespace PowerShellFar;

internal class VariableArea() : PSVariable("__", null, ScopedItemOptions.ReadOnly)
{
	public override string Description { get => "Current area interface."; set => base.Description = value; }

	public override object? Value
	{
		set => base.Value = value;
		get
		{
			return Far.Api.Window.Kind switch
			{
				WindowKind.Dialog => Far.Api.Dialog,
				WindowKind.Editor => Far.Api.Editor,
				WindowKind.Viewer => Far.Api.Viewer,
				_ => Far.Api.Panel
			};
		}
	}
}
