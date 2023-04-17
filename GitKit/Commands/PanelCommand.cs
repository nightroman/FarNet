using FarNet;
using LibGit2Sharp;

namespace GitKit;

sealed class PanelCommand : BaseCommand
{
	readonly string _panel;

	public PanelCommand(Repository repo, string value) : base(repo)
	{
		_panel = value;
	}

	public override void Invoke()
	{
		switch (_panel)
		{
			case "branches":
				new BranchesExplorer(_repo)
					.CreatePanel()
					.Open();
				return;

			case "commits":
				new CommitsExplorer(_repo, _repo.Head)
					.CreatePanel()
					.Open();
				return;

			case "changes":
				Lib.GetExistingTip(_repo);
				new ChangesExplorer(_repo, () => Lib.GetChanges(_repo))
					.CreatePanel()
					.Open();
				return;

			default:
				throw new ModuleException($"Unknown 'panel={_panel}'.");
		}
	}
}
