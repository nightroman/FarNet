using FarNet;
using LibGit2Sharp;

namespace GitKit;

abstract class BasePanel<T> : Panel where T : BaseExplorer
{
	public Repository Repository { get; }

	public BasePanel(T explorer) : base(explorer)
	{
		Repository = explorer.Repository;
	}

	protected abstract string HelpTopic { get; }

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			// show help
			case KeyCode.F1 when key.Is():
				Host.Instance.ShowHelpTopic(HelpTopic);
				return true;

			// panel members
			case KeyCode.A when key.IsCtrl():
				var data = CurrentFile?.Data;
				if (data is not null)
				{
					Host.InvokeScript(
						"[PowerShellFar.MemberExplorer]::new($args[0]).CreatePanel().OpenChild($args[1])",
						new object[] { data, this });
				}

				return true;
		}

		return base.UIKeyPressed(key);
	}
}
