using FarNet;

namespace RightWords;

public class TheHost : ModuleHost
{
	public static TheHost Instance { get; private set; } = null!;

	public TheHost()
	{
		Instance = this;
	}
}
