using FarNet;
using System.Numerics;

namespace JsonKit.Panels;

abstract class AbcPanel(AbcExplorer explorer) : Panel(explorer)
{
	protected abstract string HelpTopic { get; }

	protected void SetView(PanelPlan plan0)
	{
		ViewMode = Far.Api.Panel is AbcPanel panel && 9 == (int)panel.ViewMode ? (PanelViewMode)9: 0;

		var plan9 = plan0.Clone();
		plan9.IsFullScreen = true;
		SetPlan((PanelViewMode)9, plan9);
	}

	void ShowHelp()
	{
		Host.Instance.ShowHelpTopic(HelpTopic);
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F1 when key.Is():
				ShowHelp();
				return true;

			case KeyCode.A when key.IsCtrl():
				return true;
		}

		return base.UIKeyPressed(key);
	}
}
