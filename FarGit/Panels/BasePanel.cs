using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Base panel that owns a git repository reference.
/// </summary>
public abstract class BasePanel(BaseExplorer explorer) : AbcPanel(explorer)
{
	public string GitDir => explorer.GitDir;

	public BaseExplorer MyExplorer => (BaseExplorer)Explorer;

	/// <summary>Opens a new <see cref="Repository"/> for the panel's git directory.</summary>
	protected Repository UseRepository() => new(GitDir);
}
