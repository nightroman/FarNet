using FarNet;

namespace GitKit;

class CommitsPanel : BasePanel<CommitsExplorer>
{
	public CommitsPanel(CommitsExplorer explorer) : base(explorer)
	{
		Title = $"{explorer.Branch.FriendlyName} branch {Repository.Info.WorkingDirectory}";
		SortMode = PanelSortMode.Unsorted;
		ViewMode = 0;

		PageLimit = 100;

		var cn = new SetColumn { Kind = "N", Name = "Commit" };
		var cm = new SetColumn { Kind = "DM", Name = "Date" };

		var plan0 = new PanelPlan { Columns = new FarColumn[] { cn } };
		SetPlan(0, plan0);

		var plan9 = new PanelPlan { Columns = new FarColumn[] { cn, cm } };
		SetPlan((PanelViewMode)9, plan9);
	}

	protected override string HelpTopic => "commits-panel";
}
