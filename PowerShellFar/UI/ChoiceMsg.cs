
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using FarNet;

namespace PowerShellFar.UI
{
	static class ChoiceMsg
	{
		static void ShowHelpForChoices(Collection<ChoiceDescription> choices)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Escape - more options, e.g. to halt the command.");
			foreach (ChoiceDescription choice in choices)
			{
				int a = choice.Label.IndexOf('&');
				if (a >= 0 && a + 1 < choice.Label.Length)
					sb.Append((choice.Label[a + 1].ToString()).ToUpperInvariant());
				else
					sb.Append(choice.Label);
				sb.Append(" - ");
				if (!string.IsNullOrEmpty(choice.HelpMessage))
					sb.AppendLine(choice.HelpMessage);
				else if (a >= 0)
					sb.AppendLine(choice.Label.Replace("&", string.Empty));
				else
					sb.AppendLine();
			}
			Far.Api.AnyViewer.ViewText(sb.ToString(), "Help", OpenMode.Modal);
		}

		static public int Show(string caption, string message, Collection<ChoiceDescription> choices)
		{
			// buttons
			string[] buttons = new string[choices.Count + 1];
			buttons[choices.Count] = "Help&?";
			for (int i = choices.Count; --i >= 0; )
				buttons[i] = choices[i].Label;

			// show
			for (; ; )
			{
				int answer = Far.Api.Message(message, caption, MessageOptions.LeftAligned, buttons);

				// [Esc]:
				if (answer < 0)
				{
					A.AskStopPipeline();
					continue;
				}

				// choise:
				if (answer < choices.Count)
					return answer;

				// help:
				ShowHelpForChoices(choices);
			}
		}
	}
}
