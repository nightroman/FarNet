using FarNet;

namespace FarGit.Panels;

/// <summary>
/// Base explorer that carries the discovered git directory path.
/// </summary>
public abstract class BaseExplorer(string gitDir, Guid typeId) : Explorer(typeId)
{
	public string GitDir => gitDir;
}
