using FarNet;
using LibGit2Sharp;
using System;
using System.IO;

namespace GitKit.About;

static class Const
{
	public const string
		BlameFile = "Blame file",
		CommitLog = "Commit log",
		CompareBranches = "Compare branches",
		CompareCommits = "Compare commits",
		CopyCommit = "Copy commit to clipboard",
		CopySha = "Copy SHA-1",
		CreateBranch = "Create branch",
		EditFile = "Edit file",
		Help = "Help",
		MergeBranch = "Merge branch",
		NoBranchName = "(no branch)",
		PushBranch = "Push branch";
}

/// <summary>
/// Name: branch friendly name // Description: tip commit info // Owner: marks
/// </summary>
class BranchFile(string name, string description, string? owner = null) : FarFile
{
	public override string Name => name;
	public override string Description => description;
	public override string? Owner => owner;
	public override FileAttributes Attributes => FileAttributes.Directory;
}

/// <summary>
/// Name: commit info // Owner: mark // LastWriteTime // CommitSha
/// </summary>
class CommitFile(string name, string? owner, DateTime lastWriteTime, string commitSha) : FarFile
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
class ChangeFile(string name, string description, TreeEntryChanges change) : FarFile
{
	public override string Name => name;
	public override string Description => description;
	public TreeEntryChanges Change => change;
}
