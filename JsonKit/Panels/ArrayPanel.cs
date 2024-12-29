using FarNet;

namespace JsonKit.Panels;

sealed class ArrayPanel : AbcPanel
{
	public ArrayPanel(ArrayExplorer explorer) : base(explorer)
	{
		Title = $"Array {explorer.Node.GetPath()}";
		SortMode = PanelSortMode.Unsorted;

		var cs = new SetColumn { Kind = "S", Name = "#", Width = 5 };
		var cn = new SetColumn { Kind = "N", Name = "Value" };

		var plan0 = new PanelPlan { Columns = [cs, cn] };
		SetPlan(0, plan0);

		SetView(plan0);
	}

	protected override string HelpTopic => "array-panel";
}
