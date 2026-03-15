using FarNet;

namespace FarGit.Panels;

/// <summary>
/// Read-only git reference guide panel.
/// Lists concepts, operations, and workflows with plain-English explanations.
/// Press Enter or F3 on any entry to read the full explanation in a viewer.
/// </summary>
public class GuidePanel : BasePanel
{
	public GuidePanel(GuideExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		var co = new SetColumn { Kind = "O", Name = "Category", Width = 12 };
		var cn = new SetColumn { Kind = "N", Name = "Topic", Width = 20 };
		var cz = new SetColumn { Kind = "Z", Name = "Summary" };

		var plan0 = new PanelPlan { Columns = [co, cn, cz] };
		SetPlan(0, plan0);
		SetView(plan0);

		SetKeyBars([
			new KeyBar(KeyCode.F3, ControlKeyStates.None, "Read", "Read full explanation"),
		]);
	}

	protected override string HelpTopic => "guide-panel";

	void ShowDetail()
	{
		if (CurrentFile is not GuideFile f) return;

		var tmp = Path.ChangeExtension(Path.GetTempFileName(), ".txt");
		File.WriteAllText(tmp, f.Detail, System.Text.Encoding.UTF8);

		var viewer = Far.Api.CreateViewer();
		viewer.FileName = tmp;
		viewer.Title = $"{f.Owner}: {f.Name}";
		viewer.DeleteSource = DeleteSource.File;
		viewer.Open();
	}

	internal override void AddMenu(IMenu menu)
	{
		menu.Add("&Read explanation", (_, _) => ShowDetail());
	}

	public override void UIOpenFile(FarFile file)
	{
		if (file is GuideFile) ShowDetail();
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F3 when key.Is():
				ShowDetail();
				return true;
		}
		return base.UIKeyPressed(key);
	}
}
