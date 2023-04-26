using FarNet;

namespace GitKit;

abstract class AnyPanel : Panel
{
	public AnyPanel(Explorer explorer) : base(explorer)
	{
	}

	public abstract void AddMenu(IMenu menu);
}
