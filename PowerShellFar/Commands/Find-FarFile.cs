/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Find-FarFile command.
	/// Finds a panel file and sets it current.
	/// </summary>
	/// <seealso cref="IPanel.GoToName(string)"/>
	/// <seealso cref="IPanel.GoToName(string, bool)"/>
	[Description("Finds a panel file and sets it current.")]
	public sealed class FindFarFileCommand : BaseCmdlet
	{
		/// <summary>
		/// File name to find.
		/// </summary>
		[Parameter(Position = 0, Mandatory = true, HelpMessage = "File name to find.")]
		public string Name { get; set; }

		///
		protected override void BeginProcessing()
		{
			bool found = A.Far.Panel.GoToName(Name, false);
			if (!found)
				WriteError(new ErrorRecord(
					new FileNotFoundException("File is not found: '" + Name + "'."),
					"FileNotFound",
					ErrorCategory.ObjectNotFound,
					Name));
		}

	}
}
