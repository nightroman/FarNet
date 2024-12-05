using FarNet;

namespace JsonKit.Panels;

class ObjectPanel : AbcPanel
{
	public ObjectPanel(ObjectExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;
		ViewMode = 0;

		var cn = new SetColumn { Kind = "N", Name = "Name", Width = -40 };
		var cz = new SetColumn { Kind = "Z", Name = "Value" };

		var plan0 = new PanelPlan { Columns = [cn, cz] };
		SetPlan(0, plan0);

		var plan9 = plan0.Clone();
		plan9.IsFullScreen = true;
		SetPlan((PanelViewMode)9, plan9);
	}

	protected override string HelpTopic => "object-panel";
}
