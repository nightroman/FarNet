namespace FarGit;

static class Const
{
	public const string ModuleName = "FarGit";
	public const string Prefix = "fg";

	// Menu items
	public const string MenuStatus = "&Status";
	public const string MenuStash = "S&tash";
	public const string MenuTags = "&Tags";
	public const string MenuCommit = "&Commit...";
	public const string MenuAmend = "&Amend commit...";

	// Status panel
	public const string StageFile   = "&Stage";
	public const string UnstageFile = "&Unstage";
	public const string StageAll    = "Stage &all";
	public const string UnstageAll  = "Unstage &all";
	public const string EditFile    = "&Edit file";
	public const string DiffFile    = "&Diff file";
	public const string RevertFiles = "Re&vert files";

	// Stash panel
	public const string StashApply = "&Apply";
	public const string StashPop = "&Pop";
	public const string StashDrop = "&Drop";
	public const string StashCreate = "&Create stash...";
	public const string StashShow = "&Show changes";

	// Tag panel
	public const string TagCreate = "&Create tag...";
	public const string TagDelete = "&Delete tag";
	public const string TagShow = "&Show commit";

	// History (commit timeline) panel
	public const string MenuHistory = "Commit &History";

	// Branch panel
	public const string MenuBranches   = "&Branches";
	public const string BranchCheckout = "&Switch to branch";
	public const string BranchCreate   = "&Create branch...";
	public const string BranchRename   = "Re&name branch...";
	public const string BranchMerge    = "&Merge into current";
	public const string BranchDelete   = "&Delete branch";

	// Remote panel
	public const string MenuRemote  = "Re&mote";
	public const string RemoteFetch = "&Fetch";
	public const string RemotePull  = "&Pull";
	public const string RemotePush  = "&Push";
	public const string RemoteAdd   = "&Add remote...";
	public const string RemoteClone = "&Clone repository...";

	// Guide panel
	public const string MenuGuide = "Git &Reference Guide";

	// Guided workflows
	public const string GuideMe = "&Guide Me...";

	// Status categories (shown in Description column)
	public const string CatStaged = "Staged";
	public const string CatUnstaged = "Unstaged";
	public const string CatUntracked = "Untracked";
}
