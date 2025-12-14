using FarNet;
using System.Diagnostics;
using System.Management.Automation.Runspaces;

namespace PowerShellFar.UI;

internal static class AttachDebuggerDialog
{
	public const string Continue = "&Continue";
	public const string AddDebugger = "Add-&Debugger.ps1"; //! &D is same as &D in Assert-Far dialog
	public const string Cancel = "Cancel";
	private const string VSCode = "Open &VSCode";

	public static string Show(Runspace runspace)
	{
		var args = new MessageArgs
		{
			Text = $"""
			Attach a debugger to the runspace and continue.
			The target runspace: Id: {runspace.Id}, Name: "{runspace.Name}"
			""",
   			Caption = "Debugger",
			Buttons = [Continue, VSCode, AddDebugger, Cancel],
			HelpTopic = Entry.Instance.GetHelpTopic(HelpTopic.AttachDebuggerDialog),
		};

		while (true)
		{
			var i = Far.Api.Message(args);
			var r = i < 0 ? Cancel : args.Buttons[i];

			if (r == VSCode)
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = "code",
					UseShellExecute = true,
					WindowStyle = ProcessWindowStyle.Hidden
				});
				continue;
			}

			return r;
		}
	}
}
