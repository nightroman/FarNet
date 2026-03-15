using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// A file entry in the <see cref="StatusPanel"/>.
/// Name: relative file path
/// Description: category (Staged / Unstaged / Untracked)
/// Owner: one-letter status symbol (A / M / D / R / ?)
/// </summary>
public class StatusFile(string path, string category, string symbol, StatusEntry entry) : FarFile
{
	public override string Name => path;
	public override string Description => category;
	public override string? Owner => symbol;
	public StatusEntry Entry => entry;

	public bool IsStaged => category == Const.CatStaged;
	public bool IsUntracked => category == Const.CatUntracked;
}

/// <summary>
/// A file entry in the <see cref="StashPanel"/>.
/// Name: stash message
/// Owner: "stash@{n}" label
/// LastWriteTime: when the stash was created
/// </summary>
public class StashFile(int index, Stash stash) : FarFile
{
	public override string Name => stash.Message;
	public override string? Owner => $"stash@{{{index}}}";
	public override DateTime LastWriteTime => stash.WorkTree.Author.When.LocalDateTime;
	public override FileAttributes Attributes => FileAttributes.Directory;

	public Stash Stash => stash;
	public int Index => index;
}

/// <summary>
/// A file entry in the <see cref="TagPanel"/>.
/// Name: tag name
/// Description: abbreviated SHA + short message of the tagged commit (if any)
/// LastWriteTime: tagger/commit date
/// </summary>
public class TagFile(string name, string description, DateTime when, Tag tag) : FarFile
{
	public override string Name => name;
	public override string Description => description;
	public override DateTime LastWriteTime => when;
	public override FileAttributes Attributes => FileAttributes.Directory;

	public Tag Tag => tag;
}

/// <summary>
/// A file entry in the <see cref="BranchPanel"/>.
/// All display data is captured eagerly so the source Repository can be safely disposed.
/// Name: branch friendly name   Owner: "*" if current / remote name if remote
/// Description: tracking ahead/behind info for local branches
/// </summary>
public class BranchFile : FarFile
{
	readonly string _owner;
	readonly string _description;

	public BranchFile(Branch branch)
	{
		BranchName = branch.FriendlyName;
		IsCurrent  = branch.IsCurrentRepositoryHead;
		IsRemote   = branch.IsRemote;

		_owner = IsCurrent ? "*" : IsRemote ? RemoteName(branch) : "";

		if (!IsRemote && branch.IsTracking)
		{
			var td = branch.TrackingDetails;
			var parts = new List<string>();
			if (td.AheadBy  > 0) parts.Add($"+{td.AheadBy} to push");
			if (td.BehindBy > 0) parts.Add($"-{td.BehindBy} to pull");
			_description = parts.Count > 0 ? string.Join("  ", parts) : "up to date";
		}
		else
		{
			_description = string.Empty;
		}
	}

	public override string  Name        => BranchName;
	public override string? Owner       => _owner;
	public override string  Description => _description;
	public override FileAttributes Attributes =>
		IsCurrent ? FileAttributes.Directory : FileAttributes.Normal;

	/// <summary>Friendly name — use this to look up a fresh Branch from a new Repository.</summary>
	public string BranchName { get; }
	public bool   IsCurrent  { get; }
	public bool   IsRemote   { get; }

	static string RemoteName(Branch b)
	{
		var n = b.FriendlyName;
		var slash = n.IndexOf('/');
		return slash >= 0 ? n[..slash] : "remote";
	}
}

/// <summary>
/// A file entry in the <see cref="RemotePanel"/>.
/// All display data captured eagerly so the source Repository can be safely disposed.
/// Name: remote name   Owner: URL
/// </summary>
public class RemoteFile(Remote remote) : FarFile
{
	public override string  Name        => RemoteName;
	public override string? Owner       => RemoteUrl;
	public override string  Description => "fetch + push";
	public override FileAttributes Attributes => FileAttributes.Directory;

	public string RemoteName { get; } = remote.Name;
	public string RemoteUrl  { get; } = remote.Url ?? "";
}

/// <summary>
/// A read-only entry in the <see cref="GuidePanel"/>.
/// Name: concept or command   Owner: category   Description: one-line summary
/// </summary>
public class GuideFile(string category, string name, string summary, string detail) : FarFile
{
	public override string Name => name;
	public override string? Owner => category;
	public override string Description => summary;
	public override FileAttributes Attributes => FileAttributes.ReadOnly;
	public string Detail => detail;
}

/// <summary>
/// A file entry in the <see cref="CommitPanel"/>.
/// All display data captured eagerly so the source Repository can be safely disposed.
/// Use <see cref="Sha"/> to look up a fresh Commit when the full object is needed.
/// </summary>
public class CommitFile : FarFile
{
	readonly string _name;
	readonly string _owner;
	readonly DateTime _date;

	public CommitFile(Commit commit, string labels)
	{
		Sha    = commit.Sha;
		_name  = $"{commit.Sha[..7]}  {commit.MessageShort}";
		_owner = commit.Author.Name;
		_date  = commit.Author.When.LocalDateTime;
		Labels = labels;
	}

	public override string   Name          => _name;
	public override string?  Owner         => _owner;
	public override DateTime LastWriteTime => _date;
	public override string   Description   => Labels;
	public override FileAttributes Attributes =>
		string.IsNullOrEmpty(Labels) ? FileAttributes.Normal : FileAttributes.Directory;

	/// <summary>Full SHA — use to look up a fresh Commit from a new Repository.</summary>
	public string Sha    { get; }
	public string Labels { get; }
}

/// <summary>
/// Which section a <see cref="DashboardFile"/> represents.
/// </summary>
public enum DashboardSection { Branch, Branches, History, Staged, Unstaged, Untracked, Stash, Tags, Remote, Guide }

/// <summary>
/// A row in the <see cref="DashboardPanel"/> summary view.
/// Name: section label (e.g. "Staged")
/// Owner: count/value (e.g. "3 files")
/// Description: human-readable detail
/// </summary>
public class DashboardFile(string name, string owner, string description, DashboardSection section) : FarFile
{
	public override string Name => name;
	public override string? Owner => owner;
	public override string Description => description;
	public override FileAttributes Attributes => FileAttributes.Directory;
	public DashboardSection Section => section;
}
