using FarNet;
using FarGit.Workflows;

namespace FarGit.Panels;

/// <summary>
/// The main FarGit entry panel.
/// Shows a repo-at-a-glance: branch, branches, history, status, stash, tags, remote, guide.
///
/// Key bindings:
///   Enter    – open the relevant sub-panel
///   F2       – guided workflows (Guide Me)
///   F3       – branch manager
///   F4       – commit timeline (history)
///   F5       – stage all changes
///   F7       – open commit dialog
///   ShiftF7  – amend last commit
/// </summary>
public class DashboardPanel : BasePanel
{
	public DashboardPanel(DashboardExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		// Columns: section name | value | description
		var cn = new SetColumn { Kind = "N", Name = "Section", Width = 10 };
		var co = new SetColumn { Kind = "O", Name = "Value",   Width = 22 };
		var cz = new SetColumn { Kind = "Z", Name = "Details" };

		var plan0 = new PanelPlan { Columns = [cn, co, cz] };
		SetPlan(0, plan0);
		SetView(plan0);

		SetKeyBars([
			new KeyBar(KeyCode.F2, ControlKeyStates.None,         "Guide",    "Guided git workflows"),
			new KeyBar(KeyCode.F3, ControlKeyStates.None,         "Branches", "Branch manager"),
			new KeyBar(KeyCode.F4, ControlKeyStates.None,         "History",  "Commit timeline"),
			new KeyBar(KeyCode.F5, ControlKeyStates.None,         "StageAll", "Stage all changes"),
			new KeyBar(KeyCode.F6, ControlKeyStates.None,         "Push",     "Push commits to remote"),
			new KeyBar(KeyCode.F7, ControlKeyStates.None,         "Commit",   "Commit staged changes"),
			new KeyBar(KeyCode.F8, ControlKeyStates.None,         "",         ""),
			new KeyBar(KeyCode.F7, ControlKeyStates.ShiftPressed, "Amend",    "Amend last commit"),
		]);
	}

	protected override string HelpTopic => "dashboard";

	// --- Navigation --------------------------------------------------------

	void OpenStatus()   => new StatusExplorer(GitDir).CreatePanel().OpenChild(this);
	void OpenBranches() => new BranchExplorer(GitDir).CreatePanel().OpenChild(this);
	void OpenHistory()  => new CommitExplorer(GitDir).CreatePanel().OpenChild(this);
	void OpenStash()    => new StashExplorer(GitDir).CreatePanel().OpenChild(this);
	void OpenTags()     => new TagExplorer(GitDir).CreatePanel().OpenChild(this);
	void OpenRemote()   => new RemoteExplorer(GitDir).CreatePanel().OpenChild(this);
	void OpenGuideRef() => new GuideExplorer(GitDir).CreatePanel().OpenChild(this);
	void OpenCommit(bool amend) => Commands.Commit.Open(GitDir, amend);
	void OpenGuide() => Wizard.Show(GitDir, this);

	// --- Actions -----------------------------------------------------------

	void StageAll()
	{
		using var repo = UseRepository();
		LibGit2Sharp.Commands.Stage(repo, "*");
		Update(true);
		Redraw();
	}

	void QuickPush()
	{
		try
		{
			var output = Commands.RemoteOps.Push(GitDir);
			Update(true); Redraw();
			Far.Api.Message(output, "Push");
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	void QuickPull()
	{
		try
		{
			var output = Commands.RemoteOps.Pull(GitDir);
			Update(true); Redraw();
			Far.Api.Message(output, "Pull");
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// --- Menu / key handling -----------------------------------------------

	internal override void AddMenu(IMenu menu)
	{
		menu.Add(Const.GuideMe,       (_, _) => OpenGuide());
		menu.Add(Const.MenuGuide,     (_, _) => OpenGuideRef());
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add(Const.MenuStatus,    (_, _) => OpenStatus());
		menu.Add(Const.MenuBranches,  (_, _) => OpenBranches());
		menu.Add(Const.MenuHistory,   (_, _) => OpenHistory());
		menu.Add(Const.MenuRemote,    (_, _) => OpenRemote());
		menu.Add(Const.MenuStash,     (_, _) => OpenStash());
		menu.Add(Const.MenuTags,      (_, _) => OpenTags());
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add(Const.StageAll,      (_, _) => StageAll());
		menu.Add(Const.MenuCommit,    (_, _) => OpenCommit(false));
		menu.Add(Const.MenuAmend,     (_, _) => OpenCommit(true));
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add(Const.RemotePush,    (_, _) => QuickPush());
		menu.Add(Const.RemotePull,    (_, _) => QuickPull());
	}

	void NavigateSection(DashboardSection section)
	{
		switch (section)
		{
			case DashboardSection.Branch:
			case DashboardSection.Staged:
			case DashboardSection.Unstaged:
			case DashboardSection.Untracked:
				OpenStatus();   break;
			case DashboardSection.Branches: OpenBranches(); break;
			case DashboardSection.History:  OpenHistory();  break;
			case DashboardSection.Stash:    OpenStash();    break;
			case DashboardSection.Tags:     OpenTags();     break;
			case DashboardSection.Remote:   OpenRemote();   break;
			case DashboardSection.Guide:    OpenGuideRef(); break;
		}
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.Enter when key.Is():
				if (CurrentFile is DashboardFile df)
					NavigateSection(df.Section);
				return true;

			case KeyCode.F2 when key.Is():
				OpenGuide();
				return true;

			case KeyCode.F3 when key.Is():
				OpenBranches();
				return true;

			case KeyCode.F4 when key.Is():
				OpenHistory();
				return true;

			case KeyCode.F5 when key.Is():
				StageAll();
				return true;

			case KeyCode.F6 when key.Is():
				QuickPush();
				return true;

			case KeyCode.F8 when key.Is():
				return true; // suppress default delete

			case KeyCode.F7 when key.Is():
				OpenCommit(false);
				return true;

			case KeyCode.F7 when key.IsShift():
				OpenCommit(true);
				return true;
		}

		return base.UIKeyPressed(key);
	}
}

