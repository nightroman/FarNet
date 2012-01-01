
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class FindFarFileCommand : BaseCmdlet
	{
		[Parameter(Position = 0, Mandatory = true, ParameterSetName = "Name")]
		public string Name { get; set; }
		[Parameter(Position = 0, Mandatory = true, ParameterSetName = "Where")]
		public ScriptBlock Where { get; set; }
		[Parameter(ParameterSetName = "Where")]
		public SwitchParameter Up { get; set; }
		protected override void BeginProcessing()
		{
			if (Name != null)
			{
				bool found = Far.Net.Panel.GoToName(Name, false);
				if (!found)
					WriteError(new ErrorRecord(
						new FileNotFoundException("File is not found: '" + Name + "'."),
						"FileNotFound",
						ErrorCategory.ObjectNotFound,
						Name));
			}
			else
			{
				IList<FarFile> files = Far.Net.Panel.ShownList;
				int current = Far.Net.Panel.CurrentIndex;
				int count = files.Count;

				int step;
				int[] beg;
				int[] end;
				if (Up)
				{
					step = -1;
					beg = new int[] { current - 1, count - 1 };
					end = new int[] { -1, current - 1 };
				}
				else
				{
					step = 1;
					beg = new int[] { current + 1, 0 };
					end = new int[] { count, current + 1 };
				}

				for (int pass = 0; pass < 2; ++pass)
				{
					for (int index = beg[pass]; index != end[pass]; index += step)
					{
						SessionState.PSVariable.Set("_", files[index]);
						if (LanguagePrimitives.IsTrue(Where.InvokeReturnAsIs(null)))
						{
							Far.Net.Panel.Redraw(index, -1);
							return;
						}
					}
				}

				WriteError(new ErrorRecord(
					new FileNotFoundException("File is not found: {" + Where + "}."),
					"FileNotFound",
					ErrorCategory.ObjectNotFound,
					Where));
			}
		}
	}
}
