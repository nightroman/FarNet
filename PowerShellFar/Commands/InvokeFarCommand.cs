
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System.Management.Automation;
using System.Linq;

namespace PowerShellFar.Commands
{
	sealed class InvokeFarCommand : BaseCmdlet
	{
		[Parameter]
		public string Title { get; set; }
		[Parameter]
		public string History { get; set; }
		[Parameter]
		public string[] Prompt { get; set; }
		static string[] GetPrompt()
		{
			using (var ps = A.Psf.NewPowerShell())
			{
				var r = ps.AddCommand("prompt").Invoke();
				return r.Select(x => x.ToString()).ToArray();
			}
		}
		protected override void BeginProcessing()
		{
			var ui = new UI.InputDialog()
			{
				Prompt = Prompt ?? GetPrompt(),
				History = History ?? Res.History,
				Title = Title ?? Res.Me,
				UseLastHistory = true,
			};
			var code = ui.Show();
			if (!string.IsNullOrEmpty(code))
				A.Psf.Act(code, new ConsoleOutputWriter(Entry.CommandInvoke1.Prefix + ": " + code), false);
		}
	}
}
