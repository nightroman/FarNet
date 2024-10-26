using FarNet;
using GitKit.Commands;
using GitKit.Extras;
using GitKit.Panels;
using System.Data.Common;

namespace GitKit;

[ModuleTool(Name = Host.MyName, Options = ModuleToolOptions.Panels, Id = "7250a2f6-4eb1-4771-bf69-7a0bccd5c516")]
public class Tool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IMenu menu = Far.Api.CreateMenu();
		menu.Title = Host.MyName;
		menu.HelpTopic = GetHelpTopic("menu");

		if (Far.Api.Panel is AnyPanel panel)
		{
			panel.AddMenu(menu);
		}
		else
		{
			menu.Add("Commit log", CommitLog);
		}

		menu.Add("Help", (s, e) => Host.Instance.ShowHelpTopic(string.Empty));

		menu.Show();
	}

	void CommitLog(object? sender, MenuEventArgs e)
	{
		var parameters = new DbConnectionStringBuilder { { Parameter.Path, "?" } };
		using var command = new CommitsCommand(parameters);
		command.Invoke();
	}
}
