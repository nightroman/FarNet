using FarNet;
using LibGit2Sharp;

namespace GitKit.Panels;

/// <summary>
/// Name: branch friendly name // Description: tip commit info // Owner: marks
/// </summary>
public class BranchFile(string name, string description, string? owner = null) : FarFile
{
	public override string Name => name;
	public override string Description => description;
	public override string? Owner => owner;
	public override FileAttributes Attributes => FileAttributes.Directory;
}

/// <summary>
/// Name: commit info // Owner: mark // LastWriteTime // CommitSha
/// </summary>
public class CommitFile(string name, string? owner, DateTime lastWriteTime, string commitSha) : FarFile
{
	public override string Name => name;
	public override string? Owner => owner;
	public override DateTime LastWriteTime => lastWriteTime;
	public override FileAttributes Attributes => FileAttributes.Directory;
	public string CommitSha => commitSha;
}

/// <summary>
/// Name: file git path // Description: change kind // Change: data
/// </summary>
public class ChangeFile(string name, string description, TreeEntryChanges change) : FarFile
{
	public override string Name => name;
	public override string Description => description;
	public TreeEntryChanges Change => change;
}
