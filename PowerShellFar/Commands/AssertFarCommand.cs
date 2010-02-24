/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.Diagnostics;
using System.ComponentModel;
using System.Management.Automation;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Assert-Far command.
	/// Checks for a condition and displays a message if the condition is false.
	/// </summary>
	[Description("Checks for a condition and displays a message if the condition is false.")]
	public sealed class AssertFarCommand : BaseCmdlet
	{
		///
		[Parameter(Position = 0, ValueFromPipeline = true, HelpMessage = "Condition to be checked.")]
		public object InputObject { get; set; }

		///
		[Parameter(Position = 1, HelpMessage = "A message to display.")]
		public string Message { get; set; }

		///
		protected override void ProcessRecord()
		{
			if (LanguagePrimitives.IsTrue(InputObject))
				return;

			throw new RuntimeException(Message ?? "Assertion failed.");
		}

	}
}
