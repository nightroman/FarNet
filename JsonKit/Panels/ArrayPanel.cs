using FarNet;
using System.Linq;

namespace JsonKit.Panels;

class ArrayPanel : AbcPanel
{
	public ArrayPanel(ArrayExplorer explorer) : base(explorer)
	{
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

	protected override string HelpTopic => "array-panel";
}
