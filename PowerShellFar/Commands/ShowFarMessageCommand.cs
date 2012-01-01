
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class ShowFarMessageCommand : BaseCmdlet
	{
		[Parameter(Position = 0, Mandatory = true)]
		[AllowEmptyString]
		[AllowNull]
		public string Text { get; set; }
		[Parameter(Position = 1)]
		public string Caption { get; set; }
		[Parameter(Position = 2)]
		public ButtonSet Buttons { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[Parameter()]
		public string[] Choices { get; set; }
		[Parameter()]
		public string HelpTopic { get; set; }
		[Parameter()]
		public SwitchParameter Draw { get; set; }
		[Parameter()]
		public SwitchParameter LeftAligned { get; set; }
		[Parameter()]
		public SwitchParameter KeepBackground { get; set; }
		[Parameter()]
		public SwitchParameter IsError { get; set; }
		[Parameter()]
		public SwitchParameter IsWarning { get; set; }
		protected override void BeginProcessing()
		{
			if (Buttons != ButtonSet.Ok && Choices != null && Choices.Length > 0)
				throw new RuntimeException("Parameters 'Buttons' and 'Choices' cannot be used together.");

			MsgOptions options;
			if (Draw)
			{
				options = MsgOptions.Draw;
			}
			else
			{
				options = MsgOptions.None;
				switch (Buttons)
				{
					case ButtonSet.AbortRetryIgnore: options |= MsgOptions.AbortRetryIgnore; break;
					case ButtonSet.OkCancel: options |= MsgOptions.OkCancel; break;
					case ButtonSet.RetryCancel: options |= MsgOptions.RetryCancel; break;
					case ButtonSet.YesNo: options |= MsgOptions.YesNo; break;
					case ButtonSet.YesNoCancel: options |= MsgOptions.YesNoCancel; break;
				}
			}

			if (KeepBackground)
				options |= MsgOptions.KeepBackground;
			if (IsError)
				options |= MsgOptions.Error;
			if (IsWarning)
				options |= MsgOptions.Warning;
			if (LeftAligned)
				options |= MsgOptions.LeftAligned;

			int r = Far.Net.Message(Text ?? string.Empty, Caption, options, Choices, HelpTopic);
			if (!Draw && (Buttons != ButtonSet.Ok || Choices != null && Choices.Length > 0))
				WriteObject(r);
		}
	}
}
