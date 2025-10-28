using FarNet;
using PowerShellFar.Commands;
using System.Management.Automation;

namespace PowerShellFar;

internal class VariablePath() : PSVariable("_path", null, ScopedItemOptions.ReadOnly)
{
	public override string Description { get => "Cursor file, folder, provider path."; set => base.Description = value; }

	public override object? Value
	{
		set => base.Value = value;
		get
		{
			return Far.Api.Window.Kind switch
			{
				WindowKind.Editor => Far.Api.Editor!.FileName,
				WindowKind.Viewer => Far.Api.Viewer!.FileName,
				_ => BaseFileCmdlet.GetCurrentPath(Far.Api.Panel!),
			};
		}
	}
}
