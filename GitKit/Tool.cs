using FarNet;

namespace GitKit;

[ModuleTool(Name = Host.MyName, Options = ModuleToolOptions.Panels, Id = "7250a2f6-4eb1-4771-bf69-7a0bccd5c516")]
public class Tool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		var menu = Far.Api.CreateMenu();
		menu.Title = Host.MyName;
		menu.HelpTopic = GetHelpTopic("menu");

		switch (Far.Api.Panel)
		{
			case BranchesPanel panel:
				menu.Add("Push branch", (s, e) => panel.PushBranch());
				menu.Add("Merge branch", (s, e) => panel.MergeBranch());
				menu.Add("Compare branches", (s, e) => panel.CompareBranches());
				break;

			case CommitsPanel panel:
				menu.Add("Push branch", (s, e) => panel.PushBranch());
				menu.Add("Create branch", (s, e) => panel.CreateBranch());
				menu.Add("Compare commits", (s, e) => panel.CompareCommits());
				break;

			case ChangesPanel panel:
				menu.Add("Edit file", (s, e) => panel.EditChangeFile());
				break;
		}

		menu.Add("Help", (s, e) => Host.Instance.ShowHelpTopic(string.Empty));

		menu.Show();
	}
}
