
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
		string GetCode()
		{
			var ui = new UI.InputDialog()
			{
				Prompt = Prompt ?? GetPrompt(),
				History = History ?? Res.History,
				Title = Title ?? Res.Me,
				UseLastHistory = true,
			};
			return ui.Show() ? ui.Text : null;
		}
		protected override void BeginProcessing()
		{
			var code = GetCode();
			if (code != null)
			{
				A.Psf.Act(code, new ConsoleOutputWriter(Entry.CommandInvoke1.Prefix + ": " + code), false);
			}
		}
	}
}
