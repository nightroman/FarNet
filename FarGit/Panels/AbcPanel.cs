using FarNet;

namespace FarGit.Panels;

/// <summary>
/// Abstract base panel. Adds F1 help and the dual-view (normal + full-screen) helper.
/// </summary>
public abstract class AbcPanel(Explorer explorer) : Panel(explorer)
{
	protected abstract string HelpTopic { get; }

	/// <summary>
	/// Adds panel-specific menu items. Called by both the F11 tool menu and the F2 user menu.
	/// </summary>
	internal abstract void AddMenu(IMenu menu);

	/// <summary>
	/// Sets up both view-mode 0 (normal) and view-mode 9 (full-screen) from a single plan.
	/// View-mode 9 is used when the opposite panel is also an <see cref="AbcPanel"/>.
	/// </summary>
	protected void SetView(PanelPlan plan0)
	{
		ViewMode = Far.Api.Panel is AbcPanel panel && 9 == (int)panel.ViewMode
			? (PanelViewMode)9
			: 0;

		var plan9 = plan0.Clone();
		plan9.IsFullScreen = true;
		SetPlan((PanelViewMode)9, plan9);
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F1 when key.Is():
				// Pass the module directory (not the DLL path) so FAR finds FarGit.hlf
				var dir = Path.GetDirectoryName(typeof(AbcPanel).Assembly.Location)!;
				Far.Api.ShowHelp(dir, HelpTopic, HelpOptions.Path);
				return true;

			case KeyCode.F2 when key.Is():
				// User Menu — show this panel's actions directly without going through F11
				var menu = Far.Api.CreateMenu();
				menu.Title = Const.ModuleName;
				AddMenu(menu);
				menu.Show();
				return true;
		}

		return base.UIKeyPressed(key);
	}
}
