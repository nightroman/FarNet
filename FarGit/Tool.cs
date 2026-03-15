using FarNet;
using FarGit.Panels;
using LibGit2Sharp;

namespace FarGit;

[ModuleTool(Name = Const.ModuleName, Options = ModuleToolOptions.Panels, Id = "5f8a3b21-c9d4-4e67-a1f0-8b2e5d0c7f3a")]
public class Tool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		// If a FarGit panel is already open, show its context menu
		if (Far.Api.Panel is AbcPanel panel)
		{
			var menu = Far.Api.CreateMenu();
			menu.Title = Const.ModuleName;
			panel.AddMenu(menu);
			menu.Show();
			return;
		}

		OpenDashboard();
	}

	static void TryOpen(Action open)
	{
		try { open(); }
		catch (Exception ex) { Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning); }
	}

	static void OpenDashboard() => TryOpen(() =>
	{
		string? gitDir;
		try { gitDir = Lib.GetGitDir(Far.Api.CurrentDirectory); }
		catch { gitDir = null; }

		if (gitDir is null)
		{
			// Not in a git repo — offer helpful options instead of a bare error
			var menu = Far.Api.CreateMenu();
			menu.Title = "FarGit — No repository found here";
			menu.Add("Clone a repository from URL...");
			menu.Add("Initialize a new git repo in the current folder");
			if (!menu.Show()) return;

			if (menu.Selected == 0)
				Workflows.Wizard.CloneRepo(null);
			else
				TryOpen(InitAndOpen);
			return;
		}

		new DashboardExplorer(gitDir).CreatePanel().Open();
	});

	static void InitAndOpen()
	{
		var dir = Far.Api.CurrentDirectory;
		Repository.Init(dir);
		var gitDir = Lib.GetGitDir(dir);
		new DashboardExplorer(gitDir).CreatePanel().Open();
	}
}
