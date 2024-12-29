using FarNet;

namespace JsonKit.Panels;

sealed class ObjectPanel : AbcPanel
{
	public ObjectPanel(ObjectExplorer explorer) : base(explorer)
	{
		Title = $"Object {explorer.Node.GetPath()}";
		SortMode = PanelSortMode.Unsorted;

		var cn = new SetColumn { Kind = "N", Name = "Name", Width = -40 };
		var cz = new SetColumn { Kind = "Z", Name = "Value" };

		var plan0 = new PanelPlan { Columns = [cn, cz] };
		SetPlan(0, plan0);

		SetView(plan0);
	}

	protected override string HelpTopic => "object-panel";
}
