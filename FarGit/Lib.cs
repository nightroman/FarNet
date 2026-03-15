using FarNet;
using LibGit2Sharp;

namespace FarGit;

static class Lib
{
	/// <summary>
	/// Discovers the git directory from <paramref name="path"/> upward, or throws.
	/// </summary>
	public static string GetGitDir(string path)
	{
		return Repository.Discover(path) ?? throw new ModuleException($"Not a git repository: {path}");
	}

	/// <summary>
	/// Builds a <see cref="Signature"/> from the repository's configured user name and email.
	/// </summary>
	public static Signature BuildSignature(Repository repo)
	{
		return repo.Config.BuildSignature(DateTimeOffset.UtcNow);
	}

	/// <summary>
	/// Returns a short status symbol for the index (staged) side of a <see cref="FileStatus"/>.
	/// </summary>
	public static string StagedSymbol(FileStatus state)
	{
		if (state.HasFlag(FileStatus.NewInIndex)) return "A";
		if (state.HasFlag(FileStatus.ModifiedInIndex)) return "M";
		if (state.HasFlag(FileStatus.DeletedFromIndex)) return "D";
		if (state.HasFlag(FileStatus.RenamedInIndex)) return "R";
		if (state.HasFlag(FileStatus.TypeChangeInIndex)) return "T";
		return " ";
	}

	/// <summary>
	/// Returns a short status symbol for the workdir (unstaged) side of a <see cref="FileStatus"/>.
	/// </summary>
	public static string UnstagedSymbol(FileStatus state)
	{
		if (state.HasFlag(FileStatus.ModifiedInWorkdir)) return "M";
		if (state.HasFlag(FileStatus.DeletedFromWorkdir)) return "D";
		if (state.HasFlag(FileStatus.RenamedInWorkdir)) return "R";
		if (state.HasFlag(FileStatus.TypeChangeInWorkdir)) return "T";
		return " ";
	}
}
