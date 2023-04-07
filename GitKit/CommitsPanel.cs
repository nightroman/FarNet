using FarNet;

namespace GitKit;

class CommitsPanel : BasePanel<CommitsExplorer>
{
	public CommitsPanel(CommitsExplorer explorer) : base(explorer)
	{
		Title = $"Branch: {explorer.Branch.FriendlyName}";
		SortMode = PanelSortMode.Unsorted;
		ViewMode = 0;

		PageLimit = 100;

		var cn = new SetColumn { Kind = "N", Name = "Hash", Width = 8 };
		var cd = new SetColumn { Kind = "Z", Name = "Message", Width = -75 };
		var co = new SetColumn { Kind = "O", Name = "Author" };

		var plan0 = new PanelPlan { Columns = new FarColumn[] { cn, co, cd } };
		SetPlan(0, plan0);
	}

	protected override string HelpTopic => "commits-panel";
}
