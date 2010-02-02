/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
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
		static void ShowHelp(Collection<ChoiceDescription> choices)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Escape - more options, e.g. to halt the command.");
			foreach (ChoiceDescription choice in choices)
			{
				int a = choice.Label.IndexOf('&');
				if (a >= 0 && a + 1 < choice.Label.Length)
					sb.Append(Kit.ToUpper(choice.Label[a + 1].ToString()));
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
			A.Far.AnyViewer.ViewText(sb.ToString(), "Help", OpenMode.Modal);
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
				int answer = A.Far.Message(message, caption, MsgOptions.LeftAligned, buttons);

				// [Esc]:
				if (answer < 0)
				{
					answer = A.Far.Message(message, caption, MsgOptions.LeftAligned, new string[] { "&Halt command", "Cancel" });
					if (answer == 0)
						throw new PipelineStoppedException();
					continue;
				}

				// choise:
				if (answer < choices.Count)
					return answer;

				// help:
				ShowHelp(choices);
			}
		}
	}
}
