using FarNet;

namespace JsonKit.Panels;

abstract class AbcPanel(AbcExplorer explorer) : Panel(explorer)
{
	protected abstract string HelpTopic { get; }

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
