using FarNet;

namespace RedisKit;

class ListPanel : BasePanel<ListExplorer>
{
	public ListPanel(ListExplorer explorer) : base(explorer)
	{
		Title = explorer.ToString();
		SortMode = PanelSortMode.Unsorted;
		ViewMode = 0;

		var cs = new SetColumn { Kind = "S", Name = "#", Width = 5 };
		var cn = new SetColumn { Kind = "N", Name = "Item" };

		var plan0 = new PanelPlan { Columns = [cs, cn] };
		SetPlan(0, plan0);

		var plan9 = plan0.Clone();
		plan9.IsFullScreen = true;
		SetPlan((PanelViewMode)9, plan9);
	}

	protected override string HelpTopic => "list-panel";

	internal override void AddMenu(IMenu menu)
	{
	}

	public override void UICloneFile(CloneFileEventArgs args)
	{
	}

	public override void UICreateFile(CreateFileEventArgs args)
	{
	}

	public override void UIDeleteFiles(DeleteFilesEventArgs args)
	{
	}

	public override void UIRenameFile(RenameFileEventArgs args)
	{
	}
}
