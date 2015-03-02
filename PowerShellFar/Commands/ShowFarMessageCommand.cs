
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
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
		[Parameter]
		public string[] Choices { get; set; }
		[Parameter]
		public string HelpTopic { get; set; }
		[Parameter]
		public SwitchParameter Draw { get; set; }
		[Parameter]
		public SwitchParameter LeftAligned { get; set; }
		[Parameter]
		public SwitchParameter KeepBackground { get; set; }
		[Parameter]
		public SwitchParameter IsError { get; set; }
		[Parameter]
		public SwitchParameter IsWarning { get; set; }
		protected override void BeginProcessing()
		{
			if (Buttons != ButtonSet.Ok && Choices != null && Choices.Length > 0)
				throw new RuntimeException("Parameters 'Buttons' and 'Choices' cannot be used together.");

			MessageOptions options;
			if (Draw)
			{
				options = MessageOptions.Draw;
			}
			else
			{
				options = MessageOptions.None;
				switch (Buttons)
				{
					case ButtonSet.AbortRetryIgnore: options |= MessageOptions.AbortRetryIgnore; break;
					case ButtonSet.OkCancel: options |= MessageOptions.OkCancel; break;
					case ButtonSet.RetryCancel: options |= MessageOptions.RetryCancel; break;
					case ButtonSet.YesNo: options |= MessageOptions.YesNo; break;
					case ButtonSet.YesNoCancel: options |= MessageOptions.YesNoCancel; break;
				}
			}

			if (KeepBackground)
				options |= MessageOptions.KeepBackground;
			if (IsError)
				options |= MessageOptions.Error;
			if (IsWarning)
				options |= MessageOptions.Warning;
			if (LeftAligned)
				options |= MessageOptions.LeftAligned;

			int r = Far.Api.Message(Text ?? string.Empty, Caption, options, Choices, HelpTopic);
			if (!Draw && (Buttons != ButtonSet.Ok || Choices != null && Choices.Length > 0))
				WriteObject(r);
		}
	}
}
