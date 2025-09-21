using FarNet;

namespace GitKit.Panels;

public abstract class BaseExplorer(string gitDir, Guid typeId) : Explorer(typeId)
{
	public string GitDir => gitDir;
}
