using FarNet;
using FarNet.Forms;
using System.Management.Automation;

namespace PowerShellFar;

internal class VariableArea() : PSVariable("__", null, ScopedItemOptions.ReadOnly)
{
	public override string Description { get => "Current area interface."; set => base.Description = value; }

	private static object? ResolveDialog(IDialog dialog)
	{
		var s = dialog.TypeId.ToString();
		if (s == Guids.InputDialog || s == Guids.ReadCommandDialog)
			return Far.Api.Window.GetAt(Far.Api.Window.Count - 2);
		else
			return dialog;
	}

	public override object? Value
	{
		set => base.Value = value;
		get
		{
			var face = Far.Api.Window.GetAt(-1);
			return face?.WindowKind == WindowKind.Dialog ? ResolveDialog((IDialog)face) : face;
		}
	}
}
