using FarNet;

namespace RedisKit;

class SetPanel : BasePanel<SetExplorer>
{
	public SetPanel(SetExplorer explorer) : base(explorer)
	{
		Title = explorer.ToString();
		SortMode = PanelSortMode.Name;
		ViewMode = 0;

		var cn = new SetColumn { Kind = "N", Name = "Member" };

		var plan0 = new PanelPlan { Columns = [cn] };
		SetPlan(0, plan0);

		var plan9 = plan0.Clone();
		plan9.IsFullScreen = true;
		SetPlan((PanelViewMode)9, plan9);
	}

	protected override string HelpTopic => "set-panel";

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
