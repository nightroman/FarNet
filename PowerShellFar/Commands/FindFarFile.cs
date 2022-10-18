
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.IO;
using System.Management.Automation;

namespace PowerShellFar.Commands;

[Cmdlet("Find", "FarFile", DefaultParameterSetName = PSName)]
sealed class FindFarFileCommand : BaseCmdlet
{
	const string
		PSName = "Name",
		PSWhere = "Where";

	[Parameter(Position = 0, Mandatory = true, ParameterSetName = PSName)]
	public string? Name { get; set; }

	[Parameter(Position = 0, Mandatory = true, ParameterSetName = PSWhere)]
	public ScriptBlock? Where { get; set; }

	[Parameter(ParameterSetName = PSWhere)]
	public SwitchParameter Up { get; set; }

	protected override void BeginProcessing()
	{
		var panel = Far.Api.Panel;
		if (panel is null)
			throw new PSInvalidOperationException("Expected panel.");

		// case: find by name
		if (Name != null)
		{
			bool found = panel.GoToName(Name, false);
			if (!found)
				WriteError(new ErrorRecord(
					new FileNotFoundException("File is not found: '" + Name + "'."),
					"FileNotFound",
					ErrorCategory.ObjectNotFound,
					Name));
			return;
		}

		// case: find by filter
		var files = panel.Files;
		int count = files.Count;
		int current = panel.CurrentIndex;

		int step;
		int[] st;
		int[] en;
		if (Up)
		{
			step = -1;
			st = new int[] { current - 1, count - 1 };
			en = new int[] { -1, current - 1 };
		}
		else
		{
			step = 1;
			st = new int[] { current + 1, 0 };
			en = new int[] { count, current + 1 };
		}

		for (int pass = 0; pass < 2; ++pass)
		{
			for (int index = st[pass]; index != en[pass]; index += step)
			{
				var result = PS2.InvokeWithContext(Where!, files[index]);
				if (result.Count == 0)
					continue;

				if (result.Count > 1 || LanguagePrimitives.IsTrue(result[0]))
				{
					panel.Redraw(index, -1);
					return;
				}
			}
		}

		WriteError(new ErrorRecord(
			new FileNotFoundException($"File is not found: '{Where}'."),
			"FileNotFound",
			ErrorCategory.ObjectNotFound,
			Where));
	}
}
